using listener.listener.Application.Services.Interfaces;
using listener.listener.Application.Services;
using listener.listener.Domain.Configuration;

namespace listener.listener.Application;

public class ConfigureServices
{
    public static IServiceCollection AddApplicationService (this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<APISettings>(configuration.GetSection("APISettings"));
        services.AddScoped<INewsService, NewsService>();

        return services;
    }
}