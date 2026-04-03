using listener.listener.Infrastructure.Gateways.Interfaces;
using listener.listener.Application.Services.Interfaces;
using listener.listener.Domain.Configuration;
using listener.listener.Domain.Entities;
using listener.listener.Infrastructure.Repositories;


namespace listener.listener.Application.Services;

public class NewsService : INewsService
{
    private readonly IInterfaxGateway _gateway;
    private readonly RedisRepository _repository;
    private readonly APISettings _settings;
    private DateTime _lastOpenSession = DateTime.MinValue;
    private DateTime _lastGetRealtime = DateTime.MinValue;

    public NewsService(
        IInterfaxGateway gateway,
        RedisRepository storage,
        IOptions<APISettings> settings)
    {
        _gateway = gateway;
        _storage = storage;
        _settings = settings;
    }

    public async Task ExecuteInterfaxAPICalls(CancellationToken cancelToken)
    {
        if (DateTime.UtcNow - _lastOpenSession >= TimeSpan.FromMinutes(_settings.openSessionSettings.ReqInterval))
        {
            if (await _gateway.OpenSession(cancelToken))
            {
                _lastOpenSession = DateTime.UtcNow;
            }
            else return;
        }
        if (DateTime.UtcNow - _lastGetRealtime >= TimeSpan.FromMinutes(_settings.getRealtimeNewsByProduct.ReqInterval))
        {
            List<NewsItem> severalNews = await _gateway.GetRealtimeNewsByProduct(cancelToken);
            List<Task<NewsItem>> severalNewsTask = severalNews.Select(
                news => _gateway.GetEntireNewsByID(news, cancelToken)
            );
            List<NewsItem> newsItems = await Task.WhenAll(severalNewsTask);
            await _repository.SaveNews(newsItems);
        }
    }

    public async Task ExecuteCleanupOldNews(CancellationToken cancelToken)
    {
        await _repository.CleanUpOldNews(_settings.cleanupInterval, cancelToken);
    }
}