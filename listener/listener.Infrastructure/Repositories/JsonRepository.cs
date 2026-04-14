using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using listener.Domain.Entities;
using listener.Domain.Configuration;
using listener.Infrastructure.Repositories.Interfaces;

namespace listener.Infrastructure.Repositories;

public class JsonRepository : IJsonRepository
{
    private readonly ILogger<JsonRepository> _logger;
    private readonly RepositorySettings _settings;

    public JsonRepository(
        ILogger<JsonRepository> logger,
        IOptions<RepositorySettings> settings
    )
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task SaveNews(NewsItem[] newsItems, CancellationToken cancelToken) {
        try
        {
            string jsonResultFolder = _settings.jsonRepository.jsonResultFolder;
            if (!Directory.Exists(jsonResultFolder)) Directory.CreateDirectory(jsonResultFolder);
            string fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfffffff}.json";
            string filePath = Path.Combine(jsonResultFolder, fileName);

            JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(newsItems, jsonOptions);

            await File.WriteAllTextAsync(filePath, json, cancelToken);
            _logger.LogInformation("Сохранено {count} новостей в файл: {path}", newsItems.Count(), filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError("Ошибка при сохранении новостей в файл! {ex}", ex);
        }
    }
}