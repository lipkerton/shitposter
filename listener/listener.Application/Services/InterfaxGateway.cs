using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using listener.Domain.Entities;
using listener.Domain.Configuration;
using listener.Application.Services.Interfaces;

namespace listener.Infrastructure.Services;

public class InterfaxGateway : IInterfaxGateway
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<InterfaxGateway> _logger;
    private readonly APISettings _settings;

    public InterfaxGateway(
        IHttpClientFactory httpClientFactory,
        ILogger<InterfaxGateway> logger,
        IOptions<APISettings> settings
    )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _settings = settings.Value;
    }
    public async Task<bool> OpenSession(CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string APIUrl = _settings.OpenSession.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.OpenSession.XML
        );
        string xmlContent = await File.ReadAllTextAsync(xmlPath, cancelToken);
        
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", APIUrl);
        HttpResponseMessage response = await client.PostAsync(APIUrl, content, cancelToken);

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation("{Body}", responseContent);
        return response.IsSuccessStatusCode;
    }

    public async Task<NewsItem[]?> GetRealtimeNewsByProduct (CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string APIUrl = _settings.GetRealtimeNewsByProduct.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.GetRealtimeNewsByProduct.XML
        );
        string xmlContent = await File.ReadAllTextAsync(xmlPath, cancelToken);

        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", APIUrl);
        HttpResponseMessage response = await client.PostAsync(APIUrl, content, cancelToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Не удалось получить список новостей. Статус: {StatusCode}", response.StatusCode);
            return Enumerable.Empty<NewsItem>();
        }

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        
        XDocument xmlResponse = XDocument.Parse(responseContent);
        XNamespace xmlNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
        XNamespace apiNamespace = "http://ifx.ru/IFX3WebService";
        
        IEnumerable<XElement>? xmlBody = xmlResponse.Root?
            .Element(xmlNamespace + "Body")?
            .Element(apiNamespace + "grnmresp")?
            .Element(apiNamespace + "mbnl")?
            .Elements(apiNamespace + "c_nwli");

        if (xmlBody == null) return null;

        return xmlBody.Select(news => {
                string? id = news.Element(apiNamespace + "i")?.Value;
                string? header = news.Element(apiNamespace + "h")?.Value;
                DateTime pubDate = DateTime.Parse(news.Element(apiNamespace + "pd")?.Value);

                if (id is null) return null;

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
        string APIUrl = _settings.GetEntireNewsByID.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.GetEntireNewsByID.XML
        );
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

        _logger.LogInformation("Запрос на {url}", APIUrl);
        HttpResponseMessage response = await client.PostAsync(APIUrl, content, cancelToken);

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
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