// See https://aka.ms/new-console-template for more information

using Drunk.KeuVault.LetsEncrypt.Configs;
using Drunk.KeuVault.LetsEncrypt.Services;

var config = Configs.GetService();
var manager = new CertManager(config ?? throw new ArgumentNullException(nameof(config)));
await manager.RunAsync();