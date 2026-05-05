namespace ProductService.Models;

/// <summary>
/// Loại hàng hóa (Danh mục sản phẩm).
/// Ví dụ: Áo sơ mi, Quần Jean, Phụ kiện.
/// </summary>
public class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>Soft delete flag</summary>
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
