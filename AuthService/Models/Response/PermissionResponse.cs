namespace AuthService.Models.Response;

public class PermissionResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiPath { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
