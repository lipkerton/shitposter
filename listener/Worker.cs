using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Text;
using System.Xml.Linq;
using StackExchange.Redis;

namespace listener;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly CookieContainer _cookieContainer;
    private readonly INewsStorage _newsStorage;
    private DateTime _lastOpenSession = DateTime.MinValue;
    private DateTime _lastGetRealtime = DateTime.MinValue;

    public Worker (
        ILogger<Worker> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        CookieContainer cookieContainer,
        INewsStorage newsStorage
    )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _cookieContainer = cookieContainer;
        _newsStorage = newsStorage;
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

        string getEntireNewsByIdURL = _configuration.GetValue<string>(
            "ApiSettings:GetRealtimeNewsByProduct:Endpoint",
            "http://localhost:8000/IFXService.svc/GetRealtimeNewsByProduct"
        );
        string getEntireNewsByIdXML = Path.Combine(
            xmlPath,
            _configuration.GetValue<string>(
                "ApiSettings:GetRealtimeNewsByProduct:XML",
                "GetRealtimeByProduct.xml"
            )
        );
        TimeSpan cleanUpInterval = TimeSpan.FromMinutes(
            _configuration.GetValue<int>(
                "ApiSettings:CleanUpInterval",
                1440
            )
        );

        Task mainTask = RunMain(
            openSessionURL, openSessionXML, openSessionINT,
            getRealtimeNewsByProductURL, getRealtimeNewsByProductXML, getRealtimeNewsByProductINT,
            getEntireNewsByIdURL, getEntireNewsByIdXML,
            cancelToken
        );
        Task cleanupTask = RunCleanUp(
            cleanUpInterval, cancelToken
        );
        await Task.WhenAll(mainTask, cleanupTask);
    }

    private async Task RunMain(
        string openSessionURL, string openSessionXML, TimeSpan openSessionINT,
        string getRealtimeNewsByProductURL, string getRealtimeNewsByProductXML, TimeSpan getRealtimeNewsByProductINT,
        string getEntireNewsByIdURL, string getEntireNewsByIdXML,
        CancellationToken cancelToken
    )
    {
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
                    (bool IsSuccessStatusCode, IEnumerable<XElement>? newsList) getRealtimeNewsByProduct = await GetRealtimeNewsByProduct(
                        getRealtimeNewsByProductURL, getRealtimeNewsByProductXML, cancelToken
                    );

                    if (getRealtimeNewsByProduct.IsSuccessStatusCode)
                    {

                        List<Task<(string, string?, string?, string?)>>? elaborate = getRealtimeNewsByProduct.newsList?.Select(
                            news => GetEntireNewsByID(getEntireNewsByIdURL, getEntireNewsByIdXML, news, cancelToken)
                        ).ToList();
                        if (elaborate != null) {
                            #pragma warning disable CS8619
                            (string, string, string, string)[] result = await Task.WhenAll(elaborate);
                            await _newsStorage.SaveNews(result);
                        }
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
    }

    private async Task RunCleanUp(TimeSpan cleanUpInterval, CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            await Task.Delay(cleanUpInterval, cancelToken);
            try
            {
                await _newsStorage.CleanUpOldNews(cleanUpInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке новостей!");
            }
        }
    }

    private async Task<bool> OpenSession(string url, string path, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string xmlContent = await File.ReadAllTextAsync(path, cancelToken);
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", url);
        HttpResponseMessage response = await client.PostAsync(url, content, cancelToken);

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation("Ответ: {Body}", responseContent);
        return response.IsSuccessStatusCode;
    }

    private async Task<(bool, IEnumerable<XElement>?)> GetRealtimeNewsByProduct(string url, string path, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string xmlContent = await File.ReadAllTextAsync(path, cancelToken);
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");       
    
        _logger.LogInformation("Запрос на {url}", url);
        HttpResponseMessage response = await client.PostAsync(url, content, cancelToken);
    
        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        
        if (response.IsSuccessStatusCode) {
            XDocument xmlResponse = XDocument.Parse(responseContent);
            XNamespace xmlNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
            XNamespace apiNamespace = "http://ifx.ru/IFX3WebService";
            IEnumerable<XElement>? xmlBody = 
                xmlResponse.Root?
                    .Element(xmlNamespace + "Body")?
                    .Element(apiNamespace + "grnmresp")?
                    .Element(apiNamespace + "mbnl")?
                    .Elements(apiNamespace + "c_nwli");

            _logger.LogInformation("{0}", xmlBody);
            return (response.IsSuccessStatusCode, xmlBody);
        }

        return (response.IsSuccessStatusCode, null);
    }

    private async Task<(string, string?, string?, string?)> GetEntireNewsByID(string url, string path, XElement element, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        XDocument xmlDocument = XDocument.Load(path);
        XNamespace xmlNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace apiNamespace = "http://ifx.ru/IFX3WebService";
        XElement newsId = new XElement("mbnid", element.Element("i"));
        xmlDocument.Root?
            .Element(xmlNamespace + "Body")?
            .Element(apiNamespace + "genmreq")?
            .Add(newsId);

        string xmlContent = await File.ReadAllTextAsync(path, cancelToken);
        StringContent content = new StringContent(xmlDocument.ToString(), Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", url);
        HttpResponseMessage response = await client.PostAsync(url, content, cancelToken);

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        if (response.IsSuccessStatusCode)
        {
            XDocument xmlResponse = XDocument.Parse(responseContent);
            XElement? xmlBody = 
                xmlResponse.Root?
                    .Element(xmlNamespace + "Body")?
                    .Element(apiNamespace + "genmresp")?
                    .Element(apiNamespace + "mbn");
            string? newsContent = xmlBody?.Element(apiNamespace + "c")?.ToString(),
                    newsHeader  = xmlBody?.Element(apiNamespace + "h")?.ToString(),
                    newsPubDate = xmlBody?.Element(apiNamespace + "pd")?.ToString();
            return (newsId.ToString(), newsHeader, newsContent, newsPubDate);
        }
        _logger.LogInformation("Ответ: {Body}", responseContent);

        return  (newsId.ToString(), null, null, null);
    }
}
