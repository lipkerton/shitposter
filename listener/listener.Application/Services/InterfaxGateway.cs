using System.Xml.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using listener.Domain.Entities;
using listener.Domain.Configuration;
using listener.Domain.Constants;
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
        HttpClient client = _httpClientFactory.CreateClient(AppConstants.httpClientName);
        string APIUrl = _settings.openSession.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.openSession.XML
        );
        string xmlContent = await File.ReadAllTextAsync(xmlPath, cancelToken);
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, AppConstants.httpContentType);
        _logger.LogInformation(AppConstants.logRequestMessageInfo, APIUrl);
        HttpResponseMessage response = await client.PostAsync(APIUrl, content, cancelToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(AppConstants.logStatusCodeError, APIUrl, response.StatusCode);
            return response.IsSuccessStatusCode;
        }
        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation(AppConstants.logRequestMessageInfo, responseContent);
        return response.IsSuccessStatusCode;
    }

    public async Task<NewsItem[]> GetRealtimeNewsByProduct (CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(AppConstants.httpClientName);
        string APIUrl = _settings.getRealtimeNewsByProduct.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.getRealtimeNewsByProduct.XML
        );
        string xmlContent = await File.ReadAllTextAsync(xmlPath, cancelToken);
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, AppConstants.httpContentType);
        _logger.LogInformation(AppConstants.logRequestMessageInfo, APIUrl);
        HttpResponseMessage response = await client.PostAsync(APIUrl, content, cancelToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(AppConstants.logStatusCodeError, APIUrl, response.StatusCode);
            return Array.Empty<NewsItem>();
        }
        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        
        XDocument xmlResponse = XDocument.Parse(responseContent);
        XNamespace xmlNamespace = AppConstants.xmlNamespace;
        XNamespace apiNamespace = AppConstants.apiNamespace;
        IEnumerable<XElement>? xmlBody = xmlResponse.Root?
            .Element(xmlNamespace + AppConstants.xmlBodyTag)?
            .Element(apiNamespace + AppConstants.responseXmlGenmrespTag)?
            .Element(apiNamespace + AppConstants.responseXmlMbnlTag)?
            .Elements(apiNamespace + AppConstants.responseXmlC_nwliTag);
        if (xmlBody == null) {
            _logger.LogWarning(AppConstants.logEmptyNewsWarning);
            return Array.Empty<NewsItem>();
        }
        
        List<NewsItem> newsItems = new List<NewsItem>();
        foreach (XElement element in xmlBody)
        {
            string? id = element.Element(apiNamespace + AppConstants.responseXmlIndexTag)?.Value;
            string? header = element.Element(apiNamespace + AppConstants.responseXmlHeaderTag)?.Value;
            string? pubDateStr = element.Element(apiNamespace + AppConstants.responseXmlPubDateTag)?.Value; // DateTime.Parse(element.Element(apiNamespace + "pd")?.Value);
            if (!DateTime.TryParse(pubDateStr, out DateTime pubDate))
            {
                _logger.LogWarning(AppConstants.logValidationError, pubDateStr);
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
                _logger.LogWarning(AppConstants.logValidationError, validationError);
            }
        }
        return newsItems.ToArray<NewsItem>();
    }

    public async Task<NewsItem?> GetEntireNewsByID (NewsItem newsItem, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient(AppConstants.httpClientName);
        string APIUrl = _settings.getEntireNewsByID.Endpoint;
        string xmlPath = Path.Combine(
            _settings.XMLRequestsFolder,
            _settings.getEntireNewsByID.XML
        );
        XDocument xmlDocument = XDocument.Load(xmlPath);
        XNamespace xmlNamespace = AppConstants.xmlNamespace;
        XNamespace apiNamespace = AppConstants.apiNamespace;
        XElement newsId = new XElement(AppConstants.requestXmlMbnidTag, newsItem.Id);
        xmlDocument.Root?
            .Element(xmlNamespace + AppConstants.xmlBodyTag)?
            .Element(apiNamespace + AppConstants.requestXmlGenmreqTag)?
            .Add(newsId);

        string xmlContent = await File.ReadAllTextAsync(xmlPath, cancelToken);
        StringContent content = new StringContent(xmlContent.ToString(), Encoding.UTF8, AppConstants.httpContentType);

        _logger.LogInformation(AppConstants.logRequestMessageInfo, APIUrl);
        HttpResponseMessage response = await client.PostAsync(APIUrl, content, cancelToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(AppConstants.logStatusCodeError, APIUrl, response.StatusCode);
            return null;
        }

        string responseContent = await response.Content.ReadAsStringAsync(cancelToken);
        XDocument xmlResponse = XDocument.Parse(responseContent);
        XElement? xmlBody = 
            xmlResponse.Root?
                .Element(xmlNamespace + AppConstants.xmlBodyTag)?
                .Element(apiNamespace + AppConstants.responseXmlGenmrespTag)?
                .Element(apiNamespace + AppConstants.responseXmlMbnTag);
        string? newsContent = xmlBody?.Element(apiNamespace + AppConstants.responseXmlContentTag)?.ToString();

        if (newsContent == null) return newsItem; else newsItem.Content = newsContent; 
        _logger.LogInformation(AppConstants.logRequestMessageInfo, responseContent);
        return newsItem;
    }
}