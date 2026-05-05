using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

/// <summary>Admin tạo tài khoản mới thủ công.</summary>
public class CreateUserRequest
{
    [Required(ErrorMessage = "Username không được để trống")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Nếu null → gán Role mặc định 'USER'.</summary>
    public Guid? RoleId { get; set; }
}
