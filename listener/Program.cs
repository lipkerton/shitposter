using listener;
using System.Net;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
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
