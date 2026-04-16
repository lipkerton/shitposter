using StackExchange.Redis;
using listener.Domain.Entities;
using listener.Domain.Configuration;
using listener.Infrastructure.Repositories.Interfaces;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;

namespace listener.Infrastructure.Repositories;

public class RedisRepository : IRedisRepository
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redisDatabase;
    private readonly RepositorySettings _settings;
    private const string NewsHashPrefix = "news:";
    private const string SortedSetKey = "news:timeline";

    public RedisRepository(
        IConnectionMultiplexer redisConnection,
        IOptions<RepositorySettings> settings
    )
    {
        _redisConnection = redisConnection;
        _redisDatabase = redisConnection.GetDatabase();
        _settings = settings.Value;
    }

    public async Task SaveNews (NewsItem[] newsItems, CancellationToken cancelToken)
    {
        IBatch batch = _redisDatabase.CreateBatch();
        foreach (NewsItem news in newsItems)
        {
            string hashKey = $"{NewsHashPrefix}{news.Id}";
            HashEntry[] hashEntries = new HashEntry[]
            {
                new("id", news.Id),
                new("pd", news.PubDate.ToString()),
                new("header", news.Header),
                new("content", news.Content)
            };
            await batch.HashSetAsync(hashKey, hashEntries);
            long score = news.PubDate.ToUniversalTime().Ticks;
            await batch.SortedSetAddAsync(SortedSetKey, news.Id, score);
        }
        batch.Execute();
    }

    public async Task CleanupOldNews (CancellationToken cancelToken)
    {
        long minTicks = DateTime.UtcNow.Subtract(_settings.redisRepository.cleanupInterval).Ticks;
        long removed = await _redisDatabase.SortedSetRemoveRangeByScoreAsync(SortedSetKey, 0, minTicks);
    }
}