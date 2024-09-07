using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using Certes.Pkcs;
using Drunk.Cf.Dns;
using Drunk.KeuVault.LetsEncrypt.Configs;
using Directory = System.IO.Directory;

namespace Drunk.KeuVault.LetsEncrypt.Services;

public class CertManager(CertManagerConfig config)
{
    private readonly CertificateClient _certClient = new(new Uri(config.KeyVaultUrl), new DefaultAzureCredential(
        string.IsNullOrWhiteSpace(config.KeyVaultUID)
            ? null
            : new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = config.KeyVaultUID
            })
    );

    private static async Task<IAcmeContext> CreateAcmeContext(string email, bool useProduction)
    {
        var fileName = $"Data/{(useProduction?"prd":"staging")}-{email.Replace("@", string.Empty).Replace(".", string.Empty)}.pem";

        var accountPem = string.Empty;
        if (File.Exists(fileName))
            accountPem = await File.ReadAllTextAsync(fileName);

        if (!string.IsNullOrWhiteSpace(accountPem))
        {
            return new AcmeContext(useProduction
                    ? WellKnownServers.LetsEncryptV2
                    : WellKnownServers.LetsEncryptStagingV2,
                KeyFactory.FromPem(accountPem));
        }

        var acme = new AcmeContext(useProduction
            ? WellKnownServers.LetsEncryptV2
            : WellKnownServers.LetsEncryptStagingV2);

        await acme.NewAccount(email, true);
        var pemKey = acme.AccountKey.ToPem();

        var dir = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(fileName, pemKey);

        return acme;
    }

    private readonly ICloudflareDnsClient _cloudflareClient = CfDnsConfigs.Create(config.CfEmail, config.CfToken);

    private async Task<DnsRecordResult?> CreateDnsRecord(string recordName, string recordValue,
        CancellationToken token = default)
    {
        var record = new DnsRecord { Name = recordName, Content = recordValue, Type = RecordType.TXT, Ttl = 120 };
        var found = (await _cloudflareClient.FindByNameAsync(config.ZoneId, recordName)).Result.FirstOrDefault();

        found = (found is not null)
            ? (await _cloudflareClient.UpdateAsync(config.ZoneId, found.Id, record)).Result
            : (await _cloudflareClient.CreateAsync(config.ZoneId, record)).Result;

        Console.WriteLine($"DNS record created: {recordName}");
        return found;
    }

    private async Task DeleteDnsRecord(string recordId, CancellationToken token = default)
    {
        await _cloudflareClient.DeleteAsync(config.ZoneId, recordId);
        Console.WriteLine($"DNS record deleted: {recordId}");
    }

    private async Task<(IChallengeContext challenge, IOrderContext order, string txtName, string txtValue)>
        CreateOrder(IAcmeContext acme, string domain)
    {
        var order = await acme.NewOrder([domain]);

        // Handle the DNS-01 challenge
        var authz = (await order.Authorizations()).First();
        var dnsChallenge = await authz.Dns();
        var txtName = $"_acme-challenge.{domain}";
        var txtValue = acme.AccountKey.DnsTxt(dnsChallenge.Token);

        Console.WriteLine($"Order created: {domain}");
        return (dnsChallenge, order, txtName, txtValue);
    }

    private async Task<byte[]> IssueCert(IAcmeContext acme, IOrderContext order, string domain,
        CancellationToken token = default)
    {
        var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
        await order.Finalize(
            new CsrInfo
            {
                CountryName = config.CertInfo.CountryName,
                State = config.CertInfo.State,
                Locality = config.CertInfo.Locality,
                Organization = config.CertInfo.Organization,
                CommonName = domain,
            }, privateKey);

        var certChain = await order.TryDownload( cancellationToken: token);
        var pfxBuilder = certChain.ToPfx(privateKey);

        // if (!config.ProductionEnabled)
        // {
        //     var issuer = await File.ReadAllTextAsync("Data/Staging Root X1.pem", token);
        //     pfxBuilder.AddIssuer(Convert.FromBase64String(issuer.Replace("\r\n", "").Replace(" ", "")));
        // }

        var pfx = pfxBuilder.Build(GetCartName(domain), config.CfToken);

        Console.WriteLine($"Cert is issued: {domain}");
        return pfx;
    }

    private string GetCartName(string domain) => $"{domain.Replace(".", "-")}-lets";

    private async Task AddCertToVault(string domain, byte[] certBytes, CancellationToken token = default)
    {
        await _certClient.ImportCertificateAsync(new ImportCertificateOptions(
            GetCartName(domain),
            certBytes)
        {
            Enabled = true,
            Password = config.CfToken,
            Tags = { ["issuer"] = "LetsEncrypt", ["expireAt"] = DateTime.Now.AddMonths(3).AddDays(-1).ToString("O") }
        }, token);

        Console.WriteLine($"Cert {domain} is added to Key Vault: {_certClient.VaultUri} ");
    }

    private async Task CreateCertOrder(CancellationToken token = default)
    {
        var acme = await CreateAcmeContext(config.LetsEncryptEmail, config.ProductionEnabled);

        foreach (var domain in config.Domains)
        {
            DnsRecordResult? record = null;
            try
            {
                var info = await CreateOrder(acme, domain);
                record = await CreateDnsRecord(info.txtName, info.txtValue, token);
                await info.challenge.TryChallenge(token);
                var certs = await IssueCert(acme, info.order, domain, token);
                await AddCertToVault(domain, certs, token);
            }
            finally
            {
                // if (record is not null)
                //     await DeleteDnsRecord(record.Id, token);
            }
        }
    }

    public Task RunAsync(CancellationToken token = default) => CreateCertOrder(token);
}