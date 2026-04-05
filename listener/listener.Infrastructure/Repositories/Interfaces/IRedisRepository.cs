using listener.Domain.Entities;
namespace listener.Infrastructure.Repositories.Interfaces;

public interface IRedisRepository
{
    Task SaveNews(IEnumerable<NewsItem?> newsItems);
    Task CleanupOldNews(TimeSpan cleanupInterval);
}