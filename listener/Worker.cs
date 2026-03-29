using System.Collections;
using System.Text;

namespace listener;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public Worker (
        ILogger<Worker> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    protected override async Task ExecuteAsync(CancellationToken cancelToken)
    {
        string apiUrl = _configuration["ApiSettings:SOAPEndpoint"] ?? "http://localhost:8000/IFXService.svc";
        string xmlPath = _configuration["ApiSettings:RequestsFolder"] ?? "Requests";

        while (!cancelToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            try
            {
                await OpenSession(apiUrl, xmlPath, cancelToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка: {message}", ex.Message);
            }

            await Task.Delay(10000, cancelToken);
        }
    }

    private async Task OpenSession(string url, string path, CancellationToken cancelToken)
    {
        HttpClient client = _httpClientFactory.CreateClient();
        string xmlContent = await File.ReadAllTextAsync($"{path}\\request_open_session.xml", cancelToken);
        StringContent content = new StringContent(xmlContent, Encoding.UTF8, "text/xml");

        _logger.LogInformation("Запрос на {url}/OpenSession...", url);
        HttpResponseMessage response = await client.PostAsync($"{url}/OpenSession", content, cancelToken);

        string responseBody = await response.Content.ReadAsStringAsync(cancelToken);
        _logger.LogInformation("Ответ: {Body}", responseBody);
    }
}
