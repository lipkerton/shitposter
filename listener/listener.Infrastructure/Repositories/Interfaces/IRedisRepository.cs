using listener.listener.Domain.Entities;
namespace listener.listener.Infrastructure.Repositories.Interfaces;

public interface IRedisRepository
{
    Task SaveNews(List<NewsItem> newsItem);
    Task CleanUpOldNews();
}