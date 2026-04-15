using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using listener.Infrastructure.Repositories.Interfaces;
using listener.Application.Services.Interfaces;
using listener.Domain.Configuration;
using listener.Domain.Entities;


namespace listener.Application.Services;

public class NewsService : BackgroundService, INewsService
{
    private readonly IInterfaxGateway _gateway;
    private readonly IRedisRepository _repository;
    private readonly ILogger<NewsService> _logger;
    private readonly APISettings _settings;
    private DateTime _lastOpenSession = DateTime.MinValue;
    private DateTime _lastGetRealtime = DateTime.MinValue;
    private DateTime _lastCleanupInte = DateTime.MinValue;

    private bool IsAuthenticated;
    public NewsService(
        IInterfaxGateway gateway,
        IRedisRepository repository,
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
            _logger.LogInformation("Аутентификация...");
            if (IsAuthenticated)
            {
                _logger.LogInformation("Аутентификация прошла успешно!");
                _lastOpenSession = DateTime.UtcNow;
            }
            else
            {
                _logger.LogInformation("Аутентификация прошла неуспешно!");
                return;
            }
        }
        if (DateTime.UtcNow - _lastGetRealtime >= TimeSpan.FromMinutes(
            _settings.getRealtimeNewsByProduct.Interval   
        ))
        {
            if (IsAuthenticated) {
                _logger.LogInformation("Начаты запросы за новостями...");
                NewsItem[]? severalNews = await _gateway.GetRealtimeNewsByProduct(cancelToken);
                if (severalNews is not null) {
                    Task<NewsItem>[] severalNewsTask = severalNews.Select(
                        news => _gateway.GetEntireNewsByID(news, cancelToken)
                    ).Where(news => news != null);
                    NewsItem[] newsItems = await Task.WhenAll(severalNewsTask);
                    await _repository.SaveNews(newsItems);
                    _logger.LogInformation("Новости были успешно сохранены!");
                }
                _logger.LogInformation("Нет новостей за указанный период.");
            }
            else
            {
                _logger.LogInformation("Аутентификация прошла неуспешно - запросы за новостями пропущены!");
                return;
            }
        }
    }
}