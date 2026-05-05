using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

/// <summary>Người dùng tự đổi mật khẩu — yêu cầu xác nhận mật khẩu cũ.</summary>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu hiện tại không được để trống")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
    public string NewPassword { get; set; } = string.Empty;
}
