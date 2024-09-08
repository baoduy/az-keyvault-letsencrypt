using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Drunk.KeuVault.LetsEncrypt.Configs;

namespace Drunk.KeuVault.LetsEncrypt.Services;

/// <summary>
/// Helper class for managing certificates in Azure Key Vault.
/// </summary>
public class VaultHelper(CertManagerConfig config)
{
    private readonly CertificateClient _certClient = new(new Uri(config.KeyVaultUrl),
        new DefaultAzureCredential(string.IsNullOrWhiteSpace(config.KeyVaultUID)
                ? null
                : new DefaultAzureCredentialOptions { ManagedIdentityClientId = config.KeyVaultUID })
    );


    /// <summary>
    /// Gets the certificate name for a given domain.
    /// </summary>
    /// <param name="domain">The domain name.</param>
    /// <returns>The formatted certificate name.</returns>
    public string GetCertName(string domain) => $"{domain.Replace("*", "start").Replace(".", "-")}-lets";

    /// <summary>
    /// Adds a certificate to the Azure Key Vault.
    /// </summary>
    /// <param name="domain">The domain name.</param>
    /// <param name="certBytes">The certificate bytes.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddCert(string domain, byte[] certBytes, CancellationToken token = default)
    {
        await _certClient.ImportCertificateAsync(new ImportCertificateOptions(
            GetCertName(domain),
            certBytes)
        {
            Enabled = true,
            Password = config.CfToken,
            Tags =
            {
                ["issuer"] = "LetsEncrypt", ["expireAt"] = DateTimeOffset.Now.AddMonths(3).AddDays(-1).ToString("O")
            }
        }, token);

        Console.WriteLine($"Cert {domain} is added to Key Vault: {_certClient.VaultUri} ");
    }

    /// <summary>
    /// Gets the expiration date of the current certificate for a given domain.
    /// </summary>
    /// <param name="domain">The domain name.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the expiration date, or null if not found.</returns>
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