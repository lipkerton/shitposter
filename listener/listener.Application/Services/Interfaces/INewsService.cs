namespace listener.listener.Application.Services.Interfaces;

public interface INewsService
{
    Task ExecuteInterfaxAPICalls (CancellationToken cancelToken);
    Task ExecuteCleanupOldNews   (CancellationToken cancelToken);
}