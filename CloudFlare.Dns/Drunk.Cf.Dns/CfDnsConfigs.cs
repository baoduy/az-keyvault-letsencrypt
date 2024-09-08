using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

    public static IServiceCollection AddCloudflareDnsClient(this IServiceCollection services,
        Func<IServiceProvider, (string cfEmail, string cfToken)> auth)
    {
         services
            .AddRefitClient<ICloudflareDnsClient>(p =>
            {
                var (cfEmail, cfToken) = auth(p);
                return new RefitSettings
                {
                    AuthorizationHeaderValueGetter = (http, cal) =>
                    {
                        http.Headers.TryAddWithoutValidation("X-Auth-Email", cfEmail);
                        return Task.FromResult(cfToken);
                    }
                };
            })
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.cloudflare.com/client/v4"));
         return services;
    }
}