using listener.Infrastructure.Repositories.Interfaces;
using listener.Infrastructure.Services.Interfaces;
using listener.Application.Services.Interfaces;
using listener.Domain.Entities;
using Microsoft.Extensions.Configuration;


namespace listener.Application.Services;

public class NewsService : INewsService
{
    private readonly IInterfaxGateway _gateway;
    private readonly IRedisRepository _repository;
    private readonly IConfiguration _configuration;
    private DateTime _lastOpenSession = DateTime.MinValue;
    private DateTime _lastGetRealtime = DateTime.MinValue;
    private DateTime _lastCleanupInte = DateTime.MinValue;

    public NewsService(
        IInterfaxGateway gateway,
        IRedisRepository repository,
        IConfiguration configuration
    )
    {
        _gateway = gateway;
        _repository = repository;
        _configuration = configuration;
    }

    public async Task ExecuteInterfaxAPICalls(CancellationToken cancelToken)
    {
        if (DateTime.UtcNow - _lastOpenSession >= TimeSpan.FromMinutes(
            _configuration.GetValue<int>("APISettings:OpenSession:Interval", 1440))
        )
        {
            if (await _gateway.OpenSession(cancelToken))
            {
                _lastOpenSession = DateTime.UtcNow;
            }
            else return;
        }
        if (DateTime.UtcNow - _lastGetRealtime >= TimeSpan.FromMinutes(
                _configuration.GetValue<int>("APISettings:GetRealtimeNewsByID:Interval", 60)
            )
        )
        {
            IEnumerable<NewsItem?> severalNews = await _gateway.GetRealtimeNewsByProduct(cancelToken);
            IEnumerable<Task<NewsItem?>> severalNewsTask = severalNews.Select(
                news => _gateway.GetEntireNewsByID(news, cancelToken)
            ).Where(news => news != null);
            IEnumerable<NewsItem?> newsItems = await Task.WhenAll(severalNewsTask);
            await _repository.SaveNews(newsItems);
        }
    }

    public async Task ExecuteCleanupOldNews(CancellationToken cancelToken)
    {
        if (DateTime.UtcNow - _lastCleanupInte >= TimeSpan.FromMinutes(
                _configuration.GetValue<int>("APISettings:CleanupInterval")
            )
        ) 
        {
            await _repository.CleanupOldNews(
                TimeSpan.FromMinutes(
                    _configuration.GetValue<int>("APISettings:CleanupInterval")
                )
            );
        }
    }
}