using listener.Infrastructure.Repositories.Interfaces;
using listener.Infrastructure.Services.Interfaces;
using listener.Application.Services.Interfaces;
using listener.Domain.Entities;
using Microsoft.Extensions.Configuration;


namespace listener.Application.Services;

public class NewsService : INewsService, BackgroundService
{
    private readonly IInterfaxGateway _gateway;
    private readonly IRedisRepository _repository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NewsService> _logger;
    private DateTime _lastOpenSession = DateTime.MinValue;
    private DateTime _lastGetRealtime = DateTime.MinValue;
    private DateTime _lastCleanupInte = DateTime.MinValue;

    private bool IsAuthenticated;
    public NewsService(
        IInterfaxGateway gateway,
        IRedisRepository repository,
        IConfiguration configuration,
        ILogger<NewsService> logger
    )
    {
        _gateway = gateway;
        _logger = logger;
        _repository = repository;
        _configuration = configuration;
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
            _configuration.GetValue<int>("APISettings:OpenSession:Interval", 1440))
        )
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
                _configuration.GetValue<int>("APISettings:GetRealtimeNewsByID:Interval", 60)
            )
        )
        {
            if (IsAuthenticated) {
                _logger.LogInformation("Начаты запросы за новостями...");
                IEnumerable<NewsItem?> severalNews = await _gateway.GetRealtimeNewsByProduct(cancelToken);
                IEnumerable<Task<NewsItem?>> severalNewsTask = severalNews.Select(
                    news => _gateway.GetEntireNewsByID(news, cancelToken)
                ).Where(news => news != null);
                IEnumerable<NewsItem?> newsItems = await Task.WhenAll(severalNewsTask);
                await _repository.SaveNews(newsItems);
                _logger.LogInformation("Новости были успешно сохранены!");
            }
            else
            {
                _logger.LogInformation("Аутентификация прошла неуспешно - запросы за новостями пропущены!");
                return;
            }
        }
    }
}