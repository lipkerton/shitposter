using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using listener.listener.Domain.Configuration;
using listener.listener.Domain.Entities;

using listener.listener.Infrastructure.Repositories.Interfaces;
namespace listener.listener.Infrastructure.Repositories;

public class RedisRepository : IRedisRepository
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly APISettings _settings;
    private readonly IDatabase _redisDatabase;
    private const string NewsHashPrefix = "news:";
    private const string SortedSetKey = "news:timeline";

    public RedisRepository (
        IOptions<APISettings> settings,
        IConnectionMultiplexer redisConnection
    )
    {
        _settings = settings;
        _redisConnection = redisConnection;
        _redisDatabase = redisConnection.GetDatabase();
    }

    public async Task SaveNews (List<NewsItem> newsItems)
    {
        IBatch batch = _redisDatabase.CreateBatch();
        foreach ((int newsId, string newsHeader, string? newsContent, DateTime newsPubDate) in newsItems)
        {
            string hashKey = $"{NewsHashPrefix}{newsId}";
            HashEntry[] hashEntries = new HashEntry[]
            {
                new("id", newsId),
                new("pd", newsPubDate),
                new("header", newsHeader),
                new("content", newsContent)
            };
            await batch.HashSetAsync(hashKey, hashEntries);
            long score = DateTime.Parse(newsPubDate).ToUniversalTime().Ticks;
            await batch.SortedSetAddAsync(SortedSetKey, newsId, score);
        }
        batch.Execute();
    }

    public async Task CleanUpOldNews ()
    {
        long minTicks = DateTime.UtcNow.Subtract(_settings.cleanupInterval).Ticks;
        long removed = await _redisDatabase.SortedSetRemoveRangeByScoreAsync(SortedSetKey, 0, minTicks);
    }
}