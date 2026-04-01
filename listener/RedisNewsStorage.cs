using System.Data.Common;
using listener;
using StackExchange.Redis;

public interface INewsStorage
{
    Task SaveNews((string, string, string, string)[] newsItems);
    Task CleanUpOldNews(TimeSpan cleanUpInterval);
}

public class RedisNewsStorage : INewsStorage
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly IDatabase _redisDatabase;
    private const string NewsHashPrefix = "news:";
    private const string SortedSetKey = "news:timeline";

    public RedisNewsStorage (IConnectionMultiplexer redisConnection)
    {
        _redisConnection = redisConnection;
        _redisDatabase = redisConnection.GetDatabase();
    }

    public async Task SaveNews ((string, string, string, string)[] newsItems)
    {
        IBatch batch = _redisDatabase.CreateBatch();
        foreach ((string newsId, string newsHeader, string newsContent, string newsPubDate) in newsItems)
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

    public async Task CleanUpOldNews (TimeSpan cleanUpInterval)
    {
        long minTicks = DateTime.UtcNow.Subtract(cleanUpInterval).Ticks;
        long removed = await _redisDatabase.SortedSetRemoveRangeByScoreAsync(SortedSetKey, 0, minTicks);
    }
}