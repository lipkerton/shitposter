namespace listener.Domain.Entities;

public class NewsItem
{
    public required string Id { get; set; }
    public required DateTime PubDate { get; set; }
    public required string Header { get; set; }
    public string Content { get; set; } = string.Empty;
}
    