using System.ComponentModel.DataAnnotations;

namespace ProductService.Models.Request;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Tên hàng hóa không được để trống")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Mã SKU — để trống hệ thống sẽ tự động tạo</summary>
    [MaxLength(50)]
    public string? SKU { get; set; }

    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại hàng hóa không được để trống")]
    public Guid CategoryId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải >= 0")]
    public decimal CostPrice { get; set; } = 0;

    [Required(ErrorMessage = "Giá bán không được để trống")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải >= 0")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải >= 0")]
    public int StockQuantity { get; set; } = 0;

    [MaxLength(50)]
    public string Unit { get; set; } = "cái";

    [Range(0, int.MaxValue)]
    public int LowStockThreshold { get; set; } = 5;

    [MaxLength(500)]
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Danh sách URL các ảnh sản phẩm</summary>
    public List<string> ImageUrls { get; set; } = new();
}
