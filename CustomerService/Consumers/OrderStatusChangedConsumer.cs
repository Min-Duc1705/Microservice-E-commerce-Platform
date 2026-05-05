using CommonService.Events;
using CustomerService.Data;
using MassTransit;

namespace CustomerService.Consumers;

/// <summary>
/// Lắng nghe OrderStatusChangedEvent để cập nhật TotalSpent.
///
/// Luồng khi đơn → Completed:
///   TotalSpent += TotalAmount (luôn luôn, bất kể COD hay BankTransfer)
///   DebtAmount KHÔNG tự động thay đổi ở đây — Admin quản lý nợ thủ công qua API.
///
/// Luồng khi đơn → Cancelled (từ Completed):
///   TotalSpent -= TotalAmount (hoàn tác)
///
/// Lý do không tự động ghi nợ COD:
///   Nếu PaymentSucceededEvent đến trước Completed → Consumer sẽ tìm không thấy Debt
///   → Race condition → DebtAmount sai. Thiết kế đơn giản hơn: Admin tự ghi nợ thủ công.
/// </summary>
public class CustomerOrderStatusChangedConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly CustomerDbContext _dbContext;
    private readonly ILogger<CustomerOrderStatusChangedConsumer> _logger;

    public CustomerOrderStatusChangedConsumer(CustomerDbContext dbContext, ILogger<CustomerOrderStatusChangedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var ev = context.Message;

        _logger.LogInformation(
            "[CustomerService] Nhận OrderStatusChangedEvent — OrderId: {OrderId}, {OldStatus} → {NewStatus}",
            ev.OrderId, ev.OldStatus, ev.NewStatus);

        // Không có CustomerId → khách vãng lai, không có Customer record → bỏ qua
        if (!ev.CustomerId.HasValue) return;

        var customer = await _dbContext.Customers.FindAsync(ev.CustomerId.Value);
        if (customer == null)
        {
            _logger.LogWarning("[CustomerService] Không tìm thấy Customer {CustomerId}", ev.CustomerId.Value);
            return;
        }

        // ── Đơn hoàn thành ──────────────────────────────────────
        if (ev.NewStatus == "Completed" && ev.OldStatus == "Processing")
        {
            customer.TotalSpent += ev.TotalAmount;
            customer.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "[CustomerService] TotalSpent += {Amount} cho Customer {CustomerId}",
                ev.TotalAmount, customer.Id);
        }

        // ── Đơn bị hủy ──────────────────────────────────────────
        // Chỉ hoàn tác TotalSpent nếu đơn đã Completed trước đó
        else if (ev.NewStatus == "Cancelled" && ev.OldStatus == "Completed")
        {
            customer.TotalSpent = Math.Max(0, customer.TotalSpent - ev.TotalAmount);
            customer.UpdatedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "[CustomerService] Hoàn tác TotalSpent -{Amount} cho Customer {CustomerId}",
                ev.TotalAmount, customer.Id);

            // Ghi chú: DebtAmount KHÔNG tự động hoàn tác ở đây.
            // Nếu khách có nợ liên quan đơn này, Admin tự xử lý qua API ghi nợ thủ công.
        }

        await _dbContext.SaveChangesAsync();
    }
}
