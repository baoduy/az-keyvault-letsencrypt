using System.Threading.Tasks;
using Refit;

namespace Drunk.Cf.Dns;

public static class CfDnsConfigs
{
    public static ICloudflareDnsClient Create(string cfEmail, string cfToken) =>
        RestService.For<ICloudflareDnsClient>("https://api.cloudflare.com/client/v4",
            new RefitSettings
            {
                AuthorizationHeaderValueGetter = (http, cal) =>
                {
                    http.Headers.TryAddWithoutValidation("X-Auth-Email", cfEmail);
                    return Task.FromResult(cfToken);
                }
            });
}