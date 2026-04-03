using listener.listener.Domain.Entities;
namespace listener.listener.Infrastructure.Gateways.Interfaces;

public interface IInterfaxGateway
{
    Task<bool> OpenSession (CancellationToken cancelToken);
    Task<List<NewsItem>> GetRealtimeNewsByProduct (CancellationToken cancelToken);
    Task<NewsItem> GetEntireNewsByID (NewsItem newsItem, CancellationToken cancelToken);
}