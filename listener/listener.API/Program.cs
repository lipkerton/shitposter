using listener.listener.Application;
using listener.listener.Infrastructure;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();
