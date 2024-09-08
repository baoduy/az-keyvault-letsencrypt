// See https://aka.ms/new-console-template for more information

using Drunk.KeuVault.LetsEncrypt.Configs;
using Drunk.KeuVault.LetsEncrypt.Services;
using Microsoft.Extensions.DependencyInjection;

await using var services = Configs.GetServices();
var manager = services.GetRequiredService<CertManager>();
await manager.RunAsync();