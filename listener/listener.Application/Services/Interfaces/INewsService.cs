namespace listener.Application.Services.Interfaces;

public interface INewsService
{
    Task ExecuteInterfaxAPICalls (CancellationToken cancelToken);
}