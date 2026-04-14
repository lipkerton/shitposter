namespace listener.Domain.Configuration;

public class RepositorySettings
{
    public JsonRepository jsonRepository { get; set; } = new();
    public RedisRepository redisRepository { get; set; } = new();
}

public class JsonRepository
{
    public string JsonResultFolder { get; set; } = string.Empty;
    public string CleanupInterval { get; set; } = string.Empty;
}

public class RedisRepository
{
    public string RedisConnection { get; set; } = string.Empty;
    public string CleanupInterval { get; set; } = string.Empty;
}