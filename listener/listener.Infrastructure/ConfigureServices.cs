using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using listener.Infrastructure.Services.Interfaces;
using listener.Infrastructure.Repositories.Interfaces;
using listener.Infrastructure.Services;
using System.Net;
using listener.Domain.Configuration;
using StackExchange.Redis;
using listener.Infrastructure.Repositories;

namespace listener.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<IConnectionMultiplexer>(
            sp => ConnectionMultiplexer.Connect(
                configuration.GetValue<string>(
                    "Redis:RedisConnection",
                    "localhost:6379"
                )
            )
        );
        services.AddSingleton<CookieContainer>();
        services.AddSingleton<IInterfaxGateway, InterfaxGateway>();
        services.AddSingleton<IRedisRepository, RedisRepository>();
        services.AddHttpClient("SOAPClient")
            .ConfigurePrimaryHttpMessageHandler(
                sp =>
                {
                    CookieContainer cookieContainer = sp.GetRequiredService<CookieContainer>();
                    return new HttpClientHandler
                    {
                        CookieContainer = cookieContainer,
                        UseCookies = true
                    };
                }
            );
        return services;
    }

}