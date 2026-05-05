using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

public class UpdateEmailRequestDTO
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
}
