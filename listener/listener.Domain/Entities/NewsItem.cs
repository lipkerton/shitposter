namespace listener.Domain.Entities;

public class NewsItem
{
    public long Id { get; set; }
    public DateTime PubDate { get; set; }
    public required string Header { get; set; }
    public string? Content { get; set; }
}
    