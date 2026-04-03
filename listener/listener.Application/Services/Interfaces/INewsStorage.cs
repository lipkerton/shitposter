using listener.listener.Domain.Entities;
namespace listener.listener.Application.Services.Interfaces;

public interface INewsStorage
{
    Task SaveNews(List<NewsItem> newsItem);
    Task CleanUpOldNews();
}