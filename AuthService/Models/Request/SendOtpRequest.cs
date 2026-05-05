using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

public class SendOtpRequest
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;
}
