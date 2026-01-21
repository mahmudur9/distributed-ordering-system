namespace ObjectStoreService.Domain.Models;

// [Index(nameof(Url))]
public class Media : Base
{
    public required string Url { get; set; }
}