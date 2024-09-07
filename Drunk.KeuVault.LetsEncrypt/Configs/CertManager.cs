using System.ComponentModel.DataAnnotations;

namespace Drunk.KeuVault.LetsEncrypt.Configs;

public class CertInfoConfig
{
    [Required(AllowEmptyStrings = false)] public string CountryName { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string State { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string Locality { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string Organization { get;  init;} = string.Empty;

    [Required(AllowEmptyStrings = false)] public string OrganizationUnit { get;  init; } = string.Empty;
}

public class CertManagerConfig
{
    public static string Name => "CertManager";

    public bool ProductionEnabled { get; set; } = false;

    [Required(AllowEmptyStrings = false)] public string CfEmail { get;  init; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string CfToken { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string ZoneId { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string LetsEncryptEmail { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)] public string[] Domains { get; init; } = Enumerable.Empty<string>().ToArray();

    public CertInfoConfig CertInfo { get; init; } = default!;

    [Required(AllowEmptyStrings = false)] public string KeyVaultUrl { get; init; } = string.Empty;

    public string? KeyVaultUID { get; init; }
}