using System.Xml.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using listener.Domain.Entities;
using listener.Domain.Configuration;
using listener.Application.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

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
        string APIUrl = _settings.openSession.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.openSession.XML
        );
        string xmlContent = await File.ReadAllTextAsync(xmlPath, cancelToken);
        
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", APIUrl);
        HttpResponseMessage response = await client.PostAsync(APIUrl, content, cancelToken);

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation("{Body}", responseContent);
        return response.IsSuccessStatusCode;
    }

    public async Task<NewsItem[]> GetRealtimeNewsByProduct (CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string APIUrl = _settings.getRealtimeNewsByProduct.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.getRealtimeNewsByProduct.XML
        );
        string xmlContent = await File.ReadAllTextAsync(xmlPath, cancelToken);

        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}", APIUrl);
        HttpResponseMessage response = await client.PostAsync(APIUrl, content, cancelToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Не удалось получить список новостей. Статус: {StatusCode}", response.StatusCode);
            return Array.Empty<NewsItem>();
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

        if (xmlBody == null) {
            _logger.LogWarning("Нет новостей за указанный период!");
            return Array.Empty<NewsItem>();
        }
        
        List<NewsItem> newsItems = new List<NewsItem>();
        foreach (XElement element in xmlBody)
        {
            string? id = element.Element(apiNamespace + "i")?.Value;
            string? header = element.Element(apiNamespace + "h")?.Value;
            string? pubDateStr = element.Element(apiNamespace + "pd")?.Value; // DateTime.Parse(element.Element(apiNamespace + "pd")?.Value);

            if (!DateTime.TryParse(pubDateStr, out DateTime pubDate))
            {
                _logger.LogWarning("Некорректная дата, пропускаем новость. Дата: {pubDateStr}", pubDateStr);
                continue;
            }
            
            NewsItem newsItem = new NewsItem
            {
                Id = id ?? string.Empty,
                Header = header ?? string.Empty,
                PubDate = pubDate
            };
            ValidationContext validationContext = new ValidationContext(newsItem);
            List<ValidationResult> validationResults = new List<ValidationResult>();

            if (Validator.TryValidateObject(newsItem, validationContext, validationResults, validateAllProperties: true))
            {
                newsItems.Add(newsItem);
            }
            else
            {
                string validationError = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                _logger.LogWarning("Пропущена новость. Ошибки в данных: {validationError}", validationError);
            }
        }
        return newsItems.ToArray<NewsItem>();
    }

    public async Task<NewsItem> GetEntireNewsByID (NewsItem newsItem, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient("SOAPClient");
        string APIUrl = _settings.getEntireNewsByID.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.getEntireNewsByID.XML
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
        StringContent content = new StringContent(xmlContent.ToString(), Encoding.UTF8, "text/xml");

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