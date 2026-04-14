using listener.Domain.Entities;

namespace listener.Infrastructure.Repositories.Interfaces;

public interface ILogRepository
{
    Task SaveNews(NewsItem[] newsItems, CancellationToken cancelToken);
}