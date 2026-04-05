using StackExchange.Redis;
using listener.Domain.Entities;
using listener.Infrastructure.Repositories.Interfaces;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
namespace listener.Infrastructure.Repositories;

public class RedisRepository : IRedisRepository
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redisDatabase;
    private readonly IConfiguration _configuration;
    private const string NewsHashPrefix = "news:";
    private const string SortedSetKey = "news:timeline";

    public RedisRepository(
        IConnectionMultiplexer redisConnection,
        IConfiguration configuration
    )
    {
        _redisConnection = redisConnection;
        _redisDatabase = redisConnection.GetDatabase();
        _configuration = configuration;
    }

    public async Task SaveNews (IEnumerable<NewsItem?> newsItems)
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

    public async Task CleanupOldNews (TimeSpan cleanupInterval)
    {
        long minTicks = DateTime.UtcNow.Subtract(cleanupInterval).Ticks;
        long removed = await _redisDatabase.SortedSetRemoveRangeByScoreAsync(SortedSetKey, 0, minTicks);
    }
}