namespace AuthService.Models.Response;

public class LoginResponseDTO
{
    public string? AccessToken { get; set; }
    public UserLoginDto? User { get; set; }
}

public class UserLoginDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public RoleDto? Role { get; set; }

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new();
    }

    public class PermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ApiPath { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
    }
}