using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using listener.Infrastructure.Repositories.Interfaces;
using listener.Application.Services.Interfaces;
using listener.Domain.Configuration;
using listener.Domain.Entities;
using listener.Domain.Constants;


namespace listener.Application.Services;

public class NewsService : BackgroundService, INewsService
{
    private readonly IInterfaxGateway _gateway;
    private readonly IJsonRepository _repository;
    private readonly ILogger<NewsService> _logger;
    private readonly APISettings _settings;
    private DateTime _lastOpenSession = DateTime.MinValue;
    private DateTime _lastGetRealtime = DateTime.MinValue;

    private bool IsAuthenticated;
    public NewsService(
        IInterfaxGateway gateway,
        IJsonRepository repository,
        ILogger<NewsService> logger,
        IOptions<APISettings> settings
    )
    {
        _gateway = gateway;
        _logger = logger;
        _repository = repository;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            await ExecuteInterfaxAPICalls(cancelToken);
        }
    }
    public async Task ExecuteInterfaxAPICalls(CancellationToken cancelToken)
    {
        if (DateTime.UtcNow - _lastOpenSession >= TimeSpan.FromMinutes(
            _settings.openSession.Interval
        ))
        {
            IsAuthenticated = await _gateway.OpenSession(cancelToken);
            _logger.LogInformation(AppConstants.logAuthInfo);
            if (IsAuthenticated)
            {
                _logger.LogInformation(AppConstants.logAuthSuccessInfo);
                _lastOpenSession = DateTime.UtcNow;
            }
            else
            {
                _logger.LogInformation(AppConstants.logAuthFailureInfo);
                return;
            }
        }
        if (DateTime.UtcNow - _lastGetRealtime >= TimeSpan.FromMinutes(
            _settings.getRealtimeNewsByProduct.Interval   
        ))
        {
            if (!IsAuthenticated) return;
            _logger.LogInformation(AppConstants.logRequestStartInfo);
            NewsItem[] severalNews = await _gateway.GetRealtimeNewsByProduct(cancelToken);
            if (!severalNews.Any<NewsItem>())
            {
                _logger.LogWarning(AppConstants.logEmptyNewsWarning);
                return;
            }
            IEnumerable<Task<NewsItem?>> severalNewsTask = severalNews.Select(
                news => _gateway.GetEntireNewsByID(news, cancelToken)
            );
            NewsItem?[] newsItems = await Task.WhenAll(severalNewsTask);
            await _repository.SaveNews(
                newsItems.OfType<NewsItem>().ToArray(),
                cancelToken
            );
        }
    }
}