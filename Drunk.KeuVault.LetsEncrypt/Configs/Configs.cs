using Microsoft.Extensions.Configuration;

namespace Drunk.KeuVault.LetsEncrypt.Configs;

public static class Configs
{
    public static CertManagerConfig? GetService()
    {
        var config = new ConfigurationManager()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        return  config.GetSection(CertManagerConfig.Name).Get<CertManagerConfig>();
    }
}