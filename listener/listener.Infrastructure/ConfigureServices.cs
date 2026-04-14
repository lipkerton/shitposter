using System.Net;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using listener.Domain.Configuration;
using listener.Infrastructure.Repositories;
using listener.Infrastructure.Repositories.Interfaces;

namespace listener.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        RepositorySettings repositorySettings = new RepositorySettings();
        configuration.GetSection("RepositorySettings").Bind(repositorySettings);
        services.AddSingleton(repositorySettings);
        services.AddSingleton<IConnectionMultiplexer>(
            sp => ConnectionMultiplexer.Connect(
                repositorySettings.redisRepository.redisConnection
            )
        );
        services.AddScoped<IRedisRepository, RedisRepository>();
        services.AddScoped<IJsonRepository, JsonRepository>();
        services.AddScoped<ILogRepository, LogRepository>();
        return services;
    }

}