using System.ComponentModel.DataAnnotations;

namespace Drunk.KeuVault.LetsEncrypt.Configs;

/// <summary>
/// Configuration for certificate information.
/// </summary>
public class CertInfoConfig
{
    /// <summary>
    /// Gets the country name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string CountryName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the state.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// Gets the locality.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Locality { get; init; } = string.Empty;

    /// <summary>
    /// Gets the organization name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Organization { get; init; } = string.Empty;

    /// <summary>
    /// Gets the organization unit.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string OrganizationUnit { get; init; } = string.Empty;
}

/// <summary>
/// Configuration for a Cloudflare zone.
/// </summary>
public class CfZone
{
    /// <summary>
    /// Gets the zone ID.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ZoneId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the email address used for Let's Encrypt.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string LetsEncryptEmail { get; init; } = string.Empty;

    /// <summary>
    /// Gets the domains associated with the zone.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string[] Domains { get; init; } = Enumerable.Empty<string>().ToArray();
}

/// <summary>
/// Configuration for the certificate manager.
/// </summary>
public class CertManagerConfig
{
    /// <summary>
    /// Gets the name of the configuration.
    /// </summary>
    public static string Name => "CertManager";

    /// <summary>
    /// Gets or sets a value indicating whether production is enabled.
    /// </summary>
    public bool ProductionEnabled { get; set; } = false;

    /// <summary>
    /// Gets the Cloudflare email.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string CfEmail { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Cloudflare token.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string CfToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets the list of Cloudflare zones.
    /// </summary>
    public List<CfZone> Zones { get; init; } = new();

    /// <summary>
    /// Gets the certificate information configuration.
    /// </summary>
    public CertInfoConfig CertInfo { get; init; } = default!;

    /// <summary>
    /// Gets the Key Vault URL.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string KeyVaultUrl { get; init; } = string.Empty;

    /// <summary>
    /// Gets the Key Vault UID.
    /// </summary>
    public string? KeyVaultUID { get; init; }
}