using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

/// <summary>Admin cập nhật thông tin user.</summary>
public class UpdateUserRequest
{
    [Required(ErrorMessage = "Username không được để trống")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Nếu null → giữ nguyên Role cũ.</summary>
    public Guid? RoleId { get; set; }

    public bool IsActive { get; set; } = true;
}
