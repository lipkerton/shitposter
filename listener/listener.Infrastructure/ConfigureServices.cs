using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using listener.listener.Infrastructure.Gateways;
using listener.listener.Infrastructure.Repositories;
using listener.listener.Domain.Configuration;

using StackExchange.Redis;
using System.Net;
using listener.listener.Infrastructure.Repositories.Interfaces;

namespace listener.listener.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<APISettings>(configuration.GetSection("APISettings"));
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(configuration.GetValue("Redis:RedisConnection"))
        );
        services.AddSingleton<IInterfaxGateway, InterfaxGateway>();
        services.AddSingleton<IRedisRepository, RedisRepository>();
        services.AddSingleton<CookieContainer>();
        services.AddHttpClient("SOAPClient")
            .ConfigurePrimaryHttpMessageHandler(sp => {
                CookieContainer cookieContainer = sp.GetRequiredService<CookieContainer>();
                return new HttpClientHandler
                {
                    CookieContainer = cookieContainer,
                    UseCookies = true
                };
            }
        );
    }
}