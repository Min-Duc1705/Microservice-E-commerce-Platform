namespace AuthService.Models;

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // 1 Role có nhiều Users
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();

    // M-N với Permission
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
