using CommonService.Interface;

namespace CommonService.Events;

/// <summary>
/// Phát ra bởi OrderService khi Admin đánh dấu đơn hàng là "Hoàn thành".
/// Hứng bởi: ReportService (cộng dồn doanh thu).
/// </summary>
public class OrderCompletedEvent : IEvent
{
    public Guid OrderId    { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CompletedAt  { get; set; }
    public DateTime ProcessedAt  { get; set; }

    /// <summary>Danh sách sản phẩm trong đơn — cần để tính doanh thu và chi phí chính xác</summary>
    public List<OrderItemInfo> Items { get; set; } = new();
}
