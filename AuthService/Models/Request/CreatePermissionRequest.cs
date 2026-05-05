using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Request;

public class CreatePermissionRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string ApiPath { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(GET|POST|PUT|DELETE|PATCH)$",
        ErrorMessage = "Method phải là GET, POST, PUT, DELETE hoặc PATCH")]
    public string Method { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Module { get; set; } = string.Empty;
}
