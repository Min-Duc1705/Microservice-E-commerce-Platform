namespace ProductService.Models.Response;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Danh mục
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Giá
    public decimal CostPrice { get; set; }
    public decimal Price { get; set; }

    // Kho
    public int StockQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int LowStockThreshold { get; set; }

    /// <summary>true nếu StockQuantity &lt;= LowStockThreshold — dùng để hiện cảnh báo</summary>
    public bool IsLowStock { get; set; }

    // Trạng thái
    public string Status { get; set; } = string.Empty;

    // Ảnh
    public string ThumbnailUrl { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
