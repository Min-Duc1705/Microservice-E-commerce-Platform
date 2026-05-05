using ProductService.Utils.Enum;

namespace ProductService.Models;

/// <summary>
/// Entity hàng hóa theo yêu cầu Impl.md:
/// - Tên, SKU, Mô tả, Loại, Giá nhập, Giá bán, Tồn kho, Đơn vị tính, Ảnh, Trạng thái.
/// - Soft delete (không xóa vĩnh viễn để giữ lịch sử bán hàng).
/// </summary>
public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Mã SKU — có thể tự động tạo hoặc nhập tay</summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>Mô tả chi tiết sản phẩm (HTML/text từ rich text editor)</summary>
    public string Description { get; set; } = string.Empty;

    // ---------- Danh mục ----------
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    // ---------- Giá ----------
    /// <summary>Giá nhập — dùng để tính lãi/lỗ</summary>
    public decimal CostPrice { get; set; } = 0;

    /// <summary>Giá bán hiển thị cho khách hàng</summary>
    public decimal Price { get; set; } = 0;

    // ---------- Kho ----------
    /// <summary>Số lượng tồn kho hiện tại</summary>
    public int StockQuantity { get; set; } = 0;

    /// <summary>Đơn vị tính: cái, kg, hộp, v.v.</summary>
    public string Unit { get; set; } = "cái";

    /// <summary>Ngưỡng cảnh báo hàng sắp hết (dưới ngưỡng này → cảnh báo)</summary>
    public int LowStockThreshold { get; set; } = 5;

    // ---------- Trạng thái ----------
    public ProductStatus Status { get; set; } = ProductStatus.Active;

    // ---------- Ảnh ----------
    /// <summary>Ảnh đại diện chính</summary>
    public string ThumbnailUrl { get; set; } = string.Empty;

    /// <summary>Danh sách URL các ảnh sản phẩm (lưu dạng JSON string trong DB)</summary>
    public List<string> ImageUrls { get; set; } = new();

    // ---------- Audit ----------
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Soft delete — không xóa vật lý để giữ lịch sử bán hàng (Impl.md)</summary>
    public bool IsDeleted { get; set; } = false;

    // ─── Navigation ───────────────────────────────────────────
    /// <summary>Toàn bộ lịch sử biến động tồn kho của sản phẩm này.</summary>
    public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
}
