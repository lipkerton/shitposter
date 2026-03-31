using System;
using System.Net;
using System.Text;

namespace listener;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly CookieContainer _cookieContainer;

    public Worker (
        ILogger<Worker> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        CookieContainer cookieContainer
    )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _cookieContainer = cookieContainer;
    }
    protected override async Task ExecuteAsync(CancellationToken cancelToken)
    {
        string xmlPath = _configuration["ApiSettings:RequestsFolder"] ?? "Requests";

        string openSessionURL = _configuration["ApiSettings:Auth:Endpoint"] ?? "http://localhost:8000/IFXService.svc/OpenSession";
        string openSessionXML = Path.Combine(xmlPath, _configuration["ApiSettings:Auth:XML"] ?? "OpenSession.xml");
        int openSessionINT = int.Parse(_configuration["ApiSettings:Auth:Interval"] ?? "1440");

        await OpenSession(openSessionURL, openSessionXML, openSessionINT, cancelToken);

        await Task.Delay(10000, cancelToken);
    }

    private async Task OpenSession(string url, string path, int interval, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string xmlContent = await File.ReadAllTextAsync(path, cancelToken);
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", url);
        HttpResponseMessage response = await client.PostAsync(url, content, cancelToken);

        CookieCollection cookie = _cookieContainer.GetCookies(new Uri(url));

        string responseBody = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation("Куки: {Cookie}", cookie.Count);
        _logger.LogInformation("Ответ: {Body}", responseBody);
    }
}
