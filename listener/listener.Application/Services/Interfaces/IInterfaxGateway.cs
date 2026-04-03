using listener.listener.Domain.Entities;
namespace listener.listener.Application.Services.Interfaces;

public interface IInterfaxGateway
{
    Task<bool> OpenSession (CancellationToken cancelToken);
    Task<List<NewsItem>> GetRealtimeNewsByProduct (CancellationToken cancelToken);
    Task<NewsItem> GetEntireNewsByID (NewsItem newsItem, CancellationToken cancelToken);
}