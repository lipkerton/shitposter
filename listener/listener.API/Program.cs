using Serilog;
using System.Reflection;
using listener.Application;
using listener.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace listener.API;

public class Program {
    static void Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
            .AddJsonFile("appsettings.json")
            .Build();
        Serilog.Core.Logger _logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
        _logger.Information("Starting Service...");
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSerilog(
            lc => lc
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
        );
        builder.Services.AddApplicationService(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);

        builder.Services.AddHostedService<Worker>();

        IHost host = builder.Build();
        host.Run();
    }
}