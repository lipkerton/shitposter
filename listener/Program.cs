using listener;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();
