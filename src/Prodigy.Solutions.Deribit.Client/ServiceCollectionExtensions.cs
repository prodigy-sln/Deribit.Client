using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prodigy.Solutions.Deribit.Client.Authentication;

namespace Prodigy.Solutions.Deribit.Client;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeribitClient(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<RequestSignatureGenerator>();
        services.AddOptions<DeribitClientOptions>()
            .Configure(config.Bind)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<DeribitJsonRpcClient>();
        services.AddSingleton<DeribitAuthenticationSession>();
        
        return services;
    }
}
