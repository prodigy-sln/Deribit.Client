using Microsoft.Extensions.Configuration;
using Prodigy.Solutions.Deribit.Client;
using Prodigy.Solutions.Deribit.Client.Authentication;

// ReSharper disable once CheckNamespace -- Service injection
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDeribitClient(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<RequestSignatureGenerator>();
        services.AddOptions<DeribitClientOptions>()
            .Configure(config.Bind)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddScoped<DeribitJsonRpcClient>();
        services.AddScoped<DeribitAuthenticationSession>();
        services.AddTransient<RateLimitedThrottler>();

        return services;
    }
}
