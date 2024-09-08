using Certes;
using Certes.Acme;
using Drunk.KeuVault.LetsEncrypt.Configs;
using Directory = System.IO.Directory;

namespace Drunk.KeuVault.LetsEncrypt.Services;

public class CertManager(CfDnsHelper cfDnsHelper, VaultHelper vaultHelper, CertManagerConfig config)
{
    private static async Task<IAcmeContext> CreateAcmeContext(string email, bool useProduction)
    {
        var fileName =
            $"Data/{(useProduction ? "prd" : "staging")}-{email.Replace("@", string.Empty).Replace(".", string.Empty)}.pem";

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

        var certChain = await order.TryDownload(cancellationToken: token);
        var pfxBuilder = certChain.ToPfx(privateKey);

        // if (!config.ProductionEnabled)
        // {
        //     var issuer = await File.ReadAllTextAsync("Data/Staging Root X1.pem", token);
        //     pfxBuilder.AddIssuer(Convert.FromBase64String(issuer.Replace("\r\n", "").Replace(" ", "")));
        // }

        var pfx = pfxBuilder.Build(vaultHelper.GetCertName(domain), config.CfToken);

        Console.WriteLine($"Cert is issued: {domain}");
        return pfx;
    }


    private async Task CreateCertOrder(CfZone zone, CancellationToken token = default)
    {
        var acme = await CreateAcmeContext(zone.LetsEncryptEmail, config.ProductionEnabled);

        foreach (var domain in zone.Domains)
        {
            var currentCert = await vaultHelper.GetCurrentCertExpiration(domain, token);
            if (currentCert is not null && currentCert.Value > DateTimeOffset.UtcNow.AddDays(15))
            {
                Console.WriteLine($"A current cert is still valid for {domain}");
                continue;
            }

            var info = await CreateOrder(acme, domain);
            var record = await cfDnsHelper.UpsertRecord(zone.ZoneId, info.txtName, info.txtValue);
            await info.challenge.TryChallenge(token);
            var certs = await IssueCert(acme, info.order, domain, token);
            await vaultHelper.AddCert(domain, certs, token);
        }
    }

    public async Task RunAsync(CancellationToken token = default)
    {
        foreach (var zone in config.Zones)
            await CreateCertOrder(zone, token);
    }
}