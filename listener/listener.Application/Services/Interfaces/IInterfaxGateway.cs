using listener.Domain.Entities;
namespace listener.Application.Services.Interfaces;

public interface IInterfaxGateway
{
    Task<bool> OpenSession (CancellationToken cancelToken);
    Task<NewsItem[]?> GetRealtimeNewsByProduct (CancellationToken cancelToken);
    Task<NewsItem> GetEntireNewsByID (NewsItem newsItem, CancellationToken cancelToken);
}