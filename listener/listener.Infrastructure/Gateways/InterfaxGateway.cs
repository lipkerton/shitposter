using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using listener.listener.Infrastructure.Gateways.Interfaces;
using listener.listener.Domain.Configuration;
using listener.listener.Domain.Entities;
namespace listener.listener.Infrastructure.Gateways;

public class InterfaxGateway : IInterfaxGateway
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly APISettings _settings;
    private readonly ILogger<InterfaxGateway> _logger;

    public InterfaxGateway(
        IHttpClientFactory httpClientFactory,
        IOptions<APISettings> settings,
        ILogger<InterfaxGateway> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _settings = settings;
        _logger = logger;
    }
    public async Task<bool> OpenSession(CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string xmlPath = Path.Combine(_settings.requestPath, _settings.openSessionSettings.XMLReq);
        string xmlContent = await File.ReadAllText(xmlPath, cancelToken);
        
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", _settings.openSessionSettings.APIUrl);
        HttpResponseMessage response = await client.PostAsync(_settings.openSessionSettings.APIUrl, content, cancelToken);

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation("{Body}", responseContent);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<NewsItem>> GetRealtimeNewsByProduct (CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string xmlPath = Path.Combine(_settings.requestPath, _settings.getRealtimeNewsByProduct.XMLReq);
        string xmlContent = await File.ReadAllText(xmlPath, cancelToken);

        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", _settings.getRealtimeNewsByProduct.APIUrl);
        HttpResponseMessage response = await client.PostAsync(_settings.getRealtimeNewsByProduct.APIUrl, content, cancelToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Не удалось получить список новостей. Статус: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<NewsItem>();
        }

        string responseContent = await response.Content.ReadAsStringAsync(ct);
        
        XDocument xmlResponse = XDocument.Parse(responseContent);
        XNamespace xmlNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace apiNamespace = "http://ifx.ru/IFX3WebService";
        
        IEnumerable<XElement> xmlBody = xmlResponse.Root?
            .Element(xmlNamespace + "Body")?
            .Element(apiNamespace + "grnmresp")?
            .Element(apiNamespace + "mbnl")?
            .Elements(apiNamespace + "c_nwli");

        if (xmlBody == null) return Enumerable.Empty<NewsItem>();

        return xmlBody.Select(news => {
                int? id = news.Element(apiNamespace + "i")?.Value;
                string? header = news.Element(apiNamespace + "h")?.Value;
                DateTime? pubDate = news.Element(apiNamespace + "pd")?.Value;

                if (id is null)
                {
                    return null;
                }

                return new NewsItem {
                    Id = id,
                    PubDate = pubDate,
                    Header = header
                };
            }
        ).Where(news => news != null);
    }

    public async Task<NewsItem> GetEntireNewsByID (NewsItem newsItem, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string xmlPath = Path.Combine(_settings.requestPath, _settings.getEntireNewsByID.XMLReq);
        XDocument xmlDocument = XDocument.Load(xmlPath);
        XNamespace xmlNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace apiNamespace = "http://ifx.ru/IFX3WebService";
        XElement newsId = new XElement("mbnid", newsItem.Id);
        xmlDocument.Root?
            .Element(xmlNamespace + "Body")?
            .Element(apiNamespace + "genmreq")?
            .Add(newsId);

        string xmlContent = await File.ReadAllTextAsync(xmlPath, cancelToken);
        StringContent content = new StringContent(xmlDocument.ToString(), Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", url);
        HttpResponseMessage response = await client.PostAsync(_settings.getEntireNewsByID.APIUrl, content, cancelToken);

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Не удалось получить полный текст новости. Статус: {StatusCode}", response.StatusCode);
            return null;           
        }
        XDocument xmlResponse = XDocument.Parse(responseContent);
        XElement? xmlBody = 
            xmlResponse.Root?
                .Element(xmlNamespace + "Body")?
                .Element(apiNamespace + "genmresp")?
                .Element(apiNamespace + "mbn");
        string? newsContent = xmlBody?.Element(apiNamespace + "c")?.ToString();

        if (newsContent == null) return newsItem; else newsItem.Content = newsContent; 
        _logger.LogInformation("Ответ: {Body}", responseContent);
        return newsItem;
    }
}