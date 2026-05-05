namespace AuthService.Models.Request;

public class UpdatePermissionRequest
{
    public string Name { get; set; } = string.Empty;
    public string ApiPath { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}
