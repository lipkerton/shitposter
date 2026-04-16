using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
namespace listener.Domain.Entities;

public class NewsItem
{
    [Required(AllowEmptyStrings = false)]
    public string Id { get; set; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string Header { get; set; } = string.Empty;
    public required DateTime PubDate { get; set; }
    public string Content { get; set; } = string.Empty;
}
    