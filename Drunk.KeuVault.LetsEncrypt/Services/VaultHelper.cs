using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Drunk.KeuVault.LetsEncrypt.Configs;

namespace Drunk.KeuVault.LetsEncrypt.Services;

public class VaultHelper(CertManagerConfig config)
{
    private readonly CertificateClient _certClient = new(new Uri(config.KeyVaultUrl), new DefaultAzureCredential(
        string.IsNullOrWhiteSpace(config.KeyVaultUID)
            ? null
            : new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = config.KeyVaultUID
            })
    );

    public string GetCertName(string domain) => $"{domain.Replace(".", "-")}-lets";

    public async Task AddCert(string domain, byte[] certBytes, CancellationToken token = default)
    {
        await _certClient.ImportCertificateAsync(new ImportCertificateOptions(
            GetCertName(domain),
            certBytes)
        {
            Enabled = true,
            Password = config.CfToken,
            Tags = { ["issuer"] = "LetsEncrypt", ["expireAt"] = DateTimeOffset.Now.AddMonths(3).AddDays(-1).ToString("O") }
        }, token);

        Console.WriteLine($"Cert {domain} is added to Key Vault: {_certClient.VaultUri} ");
    }

    public async Task<DateTimeOffset?> GetCurrentCertExpiration(string domain, CancellationToken token = default)
    {
        var pages = _certClient.GetPropertiesOfCertificateVersionsAsync(GetCertName(domain), token)
            .AsPages(pageSizeHint: 1);

        try
        {
            await foreach (var page in pages)
            foreach (var c in page.Values)
                return c.ExpiresOn ?? DateTimeOffset.Parse(c.Tags["expireAt"]);
        }
        catch (Exception)
        {
            return null;
        }
        return null;
    }
}