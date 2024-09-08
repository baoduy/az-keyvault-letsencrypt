using Drunk.Cf.Dns;
using Drunk.KeuVault.LetsEncrypt.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Drunk.KeuVault.LetsEncrypt.Configs;

/// <summary>
/// Provides configuration and service registration for the application.
/// </summary>
public static class Configs
{
    /// <summary>
    /// Configures and builds the service provider with necessary services.
    /// </summary>
    /// <returns>A configured <see cref="ServiceProvider"/> instance.</returns>
    /// <exception cref="InvalidDataException">Thrown when the configuration section for <see cref="CertManagerConfig"/> is missing or invalid.</exception>
    public static ServiceProvider GetServices()
    {
        // Build configuration from appsettings.json and environment variables
        var config = new ConfigurationManager()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Retrieve and validate CertManagerConfig settings
        var settings = config.GetSection(CertManagerConfig.Name).Get<CertManagerConfig>() ?? throw new InvalidDataException(nameof(CertManagerConfig));

        // Register services
        var services = new ServiceCollection()
            .AddCloudflareDnsClient(p => (cfEmail: settings.CfEmail, cfToken: settings.CfToken))
            .AddSingleton<IConfiguration>(config)
            .AddSingleton(settings)
            .AddSingleton<CertManager>()
            .AddSingleton<CfDnsHelper>()
            .AddSingleton<VaultHelper>();

        // Build and return the service provider
        return services.BuildServiceProvider();
    }
}