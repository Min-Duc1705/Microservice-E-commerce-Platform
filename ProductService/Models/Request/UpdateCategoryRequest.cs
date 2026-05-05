using System.ComponentModel.DataAnnotations;

namespace ProductService.Models.Request;

public class UpdateCategoryRequest
{
    [Required(ErrorMessage = "Tên loại hàng hóa không được để trống")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
}
