using listener.Domain.Entities;

namespace listener.Infrastructure.Repositories.Interfaces;

public interface IJsonRepository
{
    Task SaveNews(NewsItem[] newsItems, CancellationToken cancelToken);
}