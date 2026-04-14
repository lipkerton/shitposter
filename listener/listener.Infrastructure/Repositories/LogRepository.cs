using Microsoft.Extensions.Logging;
using listener.Domain.Entities;
using listener.Infrastructure.Repositories.Interfaces;

namespace listener.Infrastructure.Repositories;

public class LogRepository : ILogRepository
{
    private readonly ILogger<LogRepository> _logger;

    public LogRepository(
        ILogger<LogRepository> logger
    )
    {
        _logger = logger;
    }

    public Task SaveNews(
        NewsItem[] newsItems,
        CancellationToken cancelToken
    )
    {
        foreach (NewsItem newsItem in newsItems)
        {
            _logger.LogInformation(">>> Получена новость:\nID: {Id}\nHeader: {Header}\n", newsItem.Id, newsItem.Header);

            if (!string.IsNullOrEmpty(newsItem.Content))
            {
                _logger.LogDebug("Сontent: {Content}\n", newsItem.Content);
            }
        }
        return Task.CompletedTask;
    }
}