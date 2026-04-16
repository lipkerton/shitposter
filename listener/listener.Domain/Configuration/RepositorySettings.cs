namespace listener.Domain.Configuration;

public class RepositorySettings
{
    public JsonRepositorySettings jsonRepository { get; set; } = new();
    public RedisRepositorySettings redisRepository { get; set; } = new();
}

public class JsonRepositorySettings
{
    public string jsonResultFolder { get; set; } = string.Empty;
    public int cleanupInterval { get; set; }
}

public class RedisRepositorySettings
{
    public string redisConnection { get; set; } = string.Empty;
    public int cleanupInterval { get; set; }
}