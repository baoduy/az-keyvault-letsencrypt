using System.ComponentModel.DataAnnotations;

namespace Drunk.KeuVault.LetsEncrypt.Configs;

public class CertInfoConfig
{
    [Required(AllowEmptyStrings = false)]
    public string CountryName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string State { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Locality { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Organization { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string OrganizationUnit { get; set; } = string.Empty;
}

public class CertManagerConfig
{
    public static string Name => "CertManager";

    public bool ProductionEnabled { get; set; } = false;

    [Required(AllowEmptyStrings = false)]
    public string CfEmail { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string CfToken { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string ZoneId { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string LetsEncryptEmail { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string[] Domains { get; set; } = Enumerable.Empty<string>().ToArray();

    public CertInfoConfig CertInfo { get; set; } = default!;
}