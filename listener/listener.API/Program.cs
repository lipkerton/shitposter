using Serilog;
using System.Reflection;
using listener.Application;
using listener.Infrastructure;
using Microsoft.Extensions.Configuration;
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

        builder.Logging.AddSerilog(_logger);
        builder.Services.AddApplicationService(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);

        IHost host = builder.Build();
        host.Run();
    }
}