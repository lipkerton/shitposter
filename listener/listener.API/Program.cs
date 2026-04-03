using listener;
using StackExchange.Redis;
using System.Net;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
string redisConnection = builder.Configuration.GetValue("Redis:RedisConnection", "localhost:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection)
);
builder.Services.AddSingleton<INewsStorage, RedisNewsStorage>();
builder.Services.AddSingleton<CookieContainer>();
builder.Services.AddHttpClient("SOAPClient")
    .ConfigurePrimaryHttpMessageHandler(sp => {
        CookieContainer cookieContainer = sp.GetRequiredService<CookieContainer>();
        return new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            UseCookies = true
        };
    }
);
builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();
