namespace UserService.Application.Responses;

public class TokenValidationResponse
{
    public bool Valid { get; set; }
    public IEnumerable<ClaimResponse>? Claims { get; set; }
}
public class ClaimResponse
{
    public string? Type { get; set; }
    public string? Value { get; set; }
}