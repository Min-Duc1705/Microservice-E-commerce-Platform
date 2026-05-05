using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã OTP không được để trống")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải có đúng 6 ký tự")]
    public string OtpCode { get; set; } = string.Empty;
}
