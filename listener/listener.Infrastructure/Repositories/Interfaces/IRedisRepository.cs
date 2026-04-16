using listener.Domain.Entities;
namespace listener.Infrastructure.Repositories.Interfaces;

public interface IRedisRepository
{
    Task SaveNews(NewsItem[] newsItems, CancellationToken cancelToken);
    Task CleanupOldNews(CancellationToken cancelToken);
}