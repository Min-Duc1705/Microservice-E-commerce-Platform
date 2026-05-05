using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

public class RegisterRequestDTO
{
    [Required(ErrorMessage = "Username không được để trống")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password không được để trống")]
    [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;
}
