using Serilog;
using System.Reflection;
using listener.Application;
using listener.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "LOCAL";
string? basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{env}.json", true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();
Serilog.Core.Logger _logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

_logger.Information($"Env value: {env}");
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

IHost host = builder.Build();
host.Run();