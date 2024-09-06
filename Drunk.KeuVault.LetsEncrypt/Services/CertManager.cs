using Certes;
using Certes.Acme;
using Certes.Acme.Resource;
using Certes.Pkcs;
using CloudFlareDns;
using CloudFlareDns.Objects.Record;
using Drunk.KeuVault.LetsEncrypt.Configs;

namespace Drunk.KeuVault.LetsEncrypt.Services;

public class CertManager(CertManagerConfig config)
{
    private readonly AcmeContext _acme = new(config.ProductionEnabled
        ? WellKnownServers.LetsEncryptV2
        : WellKnownServers.LetsEncryptStagingV2);

    private readonly CloudFlareDnsClient _cloudflareClient = new(config.CfToken, config.CfEmail, config.ZoneId);

    private async Task<Record> CreateDnsRecord(string recordName, string recordValue, CancellationToken token = default)
    {
        var record = await _cloudflareClient.Record.Create(recordName, recordValue, false, RecordType.TXT, 300);
        Console.WriteLine($"DNS record created: {recordName}");
        return record;
    }

    private async Task DeleteDnsRecord(string recordId, CancellationToken token = default)
    {
        await _cloudflareClient.Record.Delete(recordId);
        Console.WriteLine($"DNS record deleted: {recordId}");
    }

    private async Task<(IChallengeContext challenge, IOrderContext order, string txtName, string txtValue)>
        CreateOrder(string domain)
    {
        var order = await _acme.NewOrder(new[] { domain });

        // Handle the DNS-01 challenge
        var authz = (await order.Authorizations()).First();
        var dnsChallenge = await authz.Dns();
        var txtName = $"_acme-challenge.{domain}";
        var txtValue = _acme.AccountKey.DnsTxt(dnsChallenge.Token);

        Console.WriteLine($"Order created: {domain}");
        return (dnsChallenge, order, txtName, txtValue);
    }

    private async Task ChallengeAndWait(IChallengeContext challengeContext, CancellationToken token = default)
    {
        Challenge rs;
        do
        {
            await Task.Delay(10000, token);
            rs = await challengeContext.Validate();
            if (token.IsCancellationRequested) break;
        } while (rs.Status == ChallengeStatus.Pending);

        Console.WriteLine($"Challenge is completed {challengeContext.Type}");
    }

    private async Task<byte[]> IssueCert(IOrderContext order, string domain,
        CancellationToken token = default)
    {
        //var privateKey = KeyFactory.NewKey(KeyAlgorithm.RS256);
        var csr = new CertificationRequestBuilder();
        csr.AddName(
            $"C={config.CertInfo.CountryName}, ST={config.CertInfo.State}, L={config.CertInfo.Locality}, O={config.CertInfo.Organization}, CN={domain}");
        csr.SubjectAlternativeNames.Add(domain);

        await order.Finalize(csr.Generate());
        var cert = await order.Download();
        //var certPem = cert.ToPem();
        //var privateKeyPem = csr.Key.ToPem();
        var pfxBuilder = cert.ToPfx(csr.Key);
        var pfx = pfxBuilder.Build(domain.Replace(".", "-"), config.CfToken);
        Console.WriteLine($"Cert is issued: {domain}");
        return pfx;
    }

    private async Task CreateCertOrder(CancellationToken token = default)
    {
        var account = await _acme.NewAccount(config.LetsEncryptEmail, true);
        var pemKey = _acme.AccountKey.ToPem();

        foreach (var domain in config.Domains)
        {
            Record? record = null;
            try
            {
                var info = await CreateOrder(domain);
                record = await CreateDnsRecord(info.txtName, info.txtValue, token);
                await ChallengeAndWait(info.challenge, token);
                var certs = await IssueCert(info.order, domain, token);
                //TODO: Push this cert to valut
            }
            finally
            {
                if (record is not null)
                    await DeleteDnsRecord(record.Id, token);
            }
        }
    }

    public Task RunAsync(CancellationToken token = default) => CreateCertOrder(token);
}