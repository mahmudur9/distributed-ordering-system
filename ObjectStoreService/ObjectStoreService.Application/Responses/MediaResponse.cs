namespace ObjectStoreService.Application.Responses;

public class MediaResponse
{
    public required string Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}