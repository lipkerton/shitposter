namespace listener.listener.Domain.Entities;


public class NewsItem
{
    public string Id { get; set; }
    public DateTime PubDate { get; set; }
    public string Header { get; set; }
    public string Content { get; set; }

}