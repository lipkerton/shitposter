using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using listener.Application.Services.Interfaces;

namespace listener.API;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly INewsService _newsService;

    public Worker (
        ILogger<Worker> logger,
        INewsService newsService
    )
    {
        _logger = logger;
        _newsService = newsService;
    }
    protected override async Task ExecuteAsync(CancellationToken cancelToken)
    {
        Task mainLoop = RunMainLoop(cancelToken);
        Task cleanupLoop = RunCleanupLoop(cancelToken);
        await Task.WhenAll(mainLoop, cleanupLoop);
    }
    private async Task RunMainLoop(CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            try 
            {
                _logger.LogInformation("Начаты запросы за новостями...");
                await _newsService.ExecuteInterfaxAPICalls(cancelToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка во время запросов за новостями!");
            }
        }
    }
    private async Task RunCleanupLoop(CancellationToken cancelToken)
    {
        while (!cancelToken.IsCancellationRequested)
        {
            try 
            {
                _logger.LogInformation("Цикл очистки начат...");
                await _newsService.ExecuteCleanupOldNews(cancelToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка во время цикла очистки!");
            } 
        }
    }
}