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
    private DateTime _lastOpenSession = DateTime.MinValue;
    private DateTime _lastGetRealtime = DateTime.MinValue;

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
        string xmlPath = Path.GetFullPath(
            _configuration.GetValue(
                "ApiSettings:RequestsFolder",
                "Requests"
            )
        );

        string openSessionURL = _configuration.GetValue<string>(
            "ApiSettings:OpenSession:Endpoint",
            "http://localhost:8000/IFXService.svc/OpenSession"
        );
        string openSessionXML = Path.Combine(
            xmlPath,
            _configuration.GetValue<string>(
                "ApiSettings:OpenSession:XML",
                "OpenSession.xml"
            )
        );
        TimeSpan openSessionINT = TimeSpan.FromMinutes(
            _configuration.GetValue<int>(
                "ApiSettings:OpenSession:Interval",
                1440
            )
        );

        string getRealtimeNewsByProductURL = _configuration.GetValue<string>(
            "ApiSettings:GetRealtimeNewsByProduct:Endpoint",
            "http://localhost:8000/IFXService.svc/GetRealtimeNewsByProduct"
        );
        string getRealtimeNewsByProductXML = Path.Combine(
            xmlPath,
            _configuration.GetValue<string>(
                "ApiSettings:GetRealtimeNewsByProduct:XML",
                "GetRealtimeByProduct.xml"
            )
        );
        TimeSpan getRealtimeNewsByProductINT = TimeSpan.FromMinutes(
            _configuration.GetValue<int>(
                "ApiSettings:GetRealtimeNewsByProduct:Interval",
                60
            )
        );
        while (!cancelToken.IsCancellationRequested) {
            try 
            {
                if (DateTime.UtcNow - _lastOpenSession >= openSessionINT) {
                    bool openSession = await OpenSession(openSessionURL, openSessionXML, cancelToken);

                    if (openSession)
                    {
                        _lastOpenSession = DateTime.UtcNow;
                        _logger.LogInformation($"Сессия успешно обновлена. Следующее обновление через {openSessionINT} минут...");
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось авторизироваться. Повтор через 1 минуту...");
                        await Task.Delay(TimeSpan.FromMinutes(1), cancelToken);
                        continue;
                    }
                }
                if (DateTime.UtcNow - _lastGetRealtime >= getRealtimeNewsByProductINT) {
                    bool getRealtimeNewsByProduct = await GetRealtimeNewsByProduct(getRealtimeNewsByProductURL, getRealtimeNewsByProductXML, cancelToken);

                    if (getRealtimeNewsByProduct)
                    {
                        _lastGetRealtime = DateTime.UtcNow;
                        _logger.LogInformation($"Новости получены. Следующий запрос через {getRealtimeNewsByProductINT} минут...");                        
                    }
                    else
                    {
                        _logger.LogWarning("Не удалось получить новости. Повтор через 1 минуту...");
                        await Task.Delay(TimeSpan.FromMinutes(1), cancelToken);
                        continue;                        
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в основном цикле `Worker.ExecuteAsync`.");
            }
        }

        await Task.Delay(10000, cancelToken);
    }

    private async Task<bool> OpenSession(string url, string path, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string xmlContent = await File.ReadAllTextAsync(path, cancelToken);
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", url);
        HttpResponseMessage response = await client.PostAsync(url, content, cancelToken);

        string responseBody = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation("Ответ: {Body}", responseBody);
        return response.IsSuccessStatusCode;
    }

    private async Task<bool> GetRealtimeNewsByProduct(string url, string path, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string xmlContent = await File.ReadAllTextAsync(path, cancelToken);
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");       
    
        _logger.LogInformation("Запрос на {url}", url);
        HttpResponseMessage response = await client.PostAsync(url, content, cancelToken);
    
        string responseBody = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation("Ответ: {Body}", responseBody);
        return response.IsSuccessStatusCode;
    }
}
