using System.Net;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using listener.Domain.Configuration;
using listener.Infrastructure.Repositories;
using listener.Infrastructure.Repositories.Interfaces;

namespace listener.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IOptions<RepositorySettings> settings
    )
    {
        services.AddSingleton<IConnectionMultiplexer>(
            sp => ConnectionMultiplexer.Connect(
                settings.RedisRepository.RedisConnection
            )
        );
        services.AddScoped<IRedisRepository, RedisRepository>();
        services.AddScoped<IJsonRepository, JsonRepository>();
        services.AddScoped<ILogRepository, LogRepository>();
        return services;
    }

}