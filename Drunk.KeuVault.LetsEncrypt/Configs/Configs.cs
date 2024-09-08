using Drunk.Cf.Dns;
using Drunk.KeuVault.LetsEncrypt.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Drunk.KeuVault.LetsEncrypt.Configs;

public static class Configs
{
    public static ServiceProvider GetServices()
    {
        var config = new ConfigurationManager()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var settings = config.GetSection(CertManagerConfig.Name).Get<CertManagerConfig>() ?? throw new InvalidDataException(nameof(CertManagerConfig));

        var services = new ServiceCollection()
            .AddCloudflareDnsClient(p => (cfEmail: settings.CfEmail, cfToken: settings.CfToken))
            .AddSingleton<IConfiguration>(config)
            .AddSingleton(settings)
            .AddSingleton<CertManager>()
            .AddSingleton<CfDnsHelper>()
            .AddSingleton<VaultHelper>();

        return services.BuildServiceProvider();
    }
}