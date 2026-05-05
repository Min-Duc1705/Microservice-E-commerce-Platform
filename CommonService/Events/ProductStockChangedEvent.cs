using CommonService.Interface;

namespace CommonService.Events;

/// <summary>
/// Phát ra bởi ProductService mỗi khi tồn kho sản phẩm có thay đổi.
/// Hứng bởi: ReportService (cập nhật snapshot tồn kho).
/// </summary>
public class ProductStockChangedEvent : IEvent
{
    public Guid ProductId { get; set; }
    public string ProductName  { get; set; } = string.Empty;
    public string SKU          { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Tồn kho hiện tại sau thay đổi</summary>
    public int StockQuantity     { get; set; }

    /// <summary>Ngưỡng cảnh báo hết hàng</summary>
    public int LowStockThreshold { get; set; }

    /// <summary>Số lượng bán 30 ngày qua — để cập nhật hiệu suất bán</summary>
    public int SoldLast30Days { get; set; }

    /// <summary>Ngày bán hàng cuối cùng</summary>
    public DateTime? LastSoldAt  { get; set; }
    public DateTime  ProcessedAt { get; set; }
}
