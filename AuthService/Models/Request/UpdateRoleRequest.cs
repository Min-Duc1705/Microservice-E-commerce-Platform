using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

public class UpdateRoleRequest
{
    [Required(ErrorMessage = "Tên role không được để trống")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Nếu null → không đổi permissions. Nếu có list → replace toàn bộ.</summary>
    public List<Guid>? PermissionIds { get; set; }
}
