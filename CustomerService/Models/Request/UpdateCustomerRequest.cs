using System.ComponentModel.DataAnnotations;

namespace CustomerService.Models.Request;

public class UpdateCustomerRequest
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại không được để trống")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(15)]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Address { get; set; } = string.Empty;

    /// <summary>URL ảnh đại diện mới (để trống nếu không đổi ảnh).</summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}
