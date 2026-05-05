namespace ReportService.Models;

/// <summary>
/// Snapshot tồn kho và hiệu suất bán hàng của từng sản phẩm.
/// Cập nhật bởi ProductStockChangedConsumer mỗi khi tồn kho thay đổi.
/// Theo DB Analysis: 02.Database_Analysis.md Section 6.
/// </summary>
public class ProductStockSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Unique — trỏ tới Product bên ProductService (không FK cứng)</summary>
    public Guid ProductId { get; set; }

    // ---------- Thông tin sản phẩm (cache để tránh gọi HTTP) ----------
    public string ProductName  { get; set; } = string.Empty;
    public string SKU          { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    // ---------- Tồn kho ----------
    public int StockQuantity     { get; set; }
    public int LowStockThreshold { get; set; }

    /// <summary>Computed: không lưu DB — tính lại mỗi lần đọc</summary>
    public bool IsLowStock => StockQuantity <= LowStockThreshold;

    // ---------- Hiệu suất bán ----------
    /// <summary>Số lượng bán 30 ngày qua — để xếp hạng hàng bán chạy</summary>
    public int SoldLast30Days { get; set; }

    /// <summary>Ngày bán cuối cùng — null nếu chưa bán lần nào</summary>
    public DateTime? LastSoldAt { get; set; }

    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
