using listener;
using System.Net;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
CookieContainer cookieContainer = new CookieContainer();
builder.Services.AddHttpClient(
    "SOAPClient", client => {}
).ConfigurePrimaryHttpMessageHandler(
    () => new HttpClientHandler
    {
        CookieContainer = cookieContainer,
        UseCookies = true
    }
);
builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();
