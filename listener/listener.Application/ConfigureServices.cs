using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using listener.Application.Services.Interfaces;
using listener.Application.Services;

namespace listener.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationService (this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<INewsService, NewsService>();
        return services;
    }
}

