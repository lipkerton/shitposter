using listener.Domain.Entities;
namespace listener.Infrastructure.Services.Interfaces;

public interface IInterfaxGateway
{
    Task<bool> OpenSession (CancellationToken cancelToken);
    Task<IEnumerable<NewsItem?>> GetRealtimeNewsByProduct (CancellationToken cancelToken);
    Task<NewsItem?> GetEntireNewsByID (NewsItem newsItem, CancellationToken cancelToken);
}