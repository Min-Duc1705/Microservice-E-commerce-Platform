using CommonService.Events;
using MassTransit;
using OrderService.Services.Interface;
using OrderService.Utils.Enum;

namespace OrderService.Consumers;

/// <summary>
/// Lắng nghe PaymentSucceededEvent từ PaymentService.
///
/// Luồng BankTransfer / Momo:
///   Khách chuyển khoản → Admin xác nhận → PaymentSucceededEvent
///   → Đơn đang ở "New" → chuyển sang "Processing" (bắt đầu đóng gói/giao hàng)
///
/// Luồng COD:
///   Admin giao hàng + thu tiền mặt → Admin xác nhận → PaymentSucceededEvent
///   → Đơn đã "Completed" → không cần đổi status, chỉ CustomerService cần clear nợ
/// </summary>
public class OrderPaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderPaymentSucceededConsumer> _logger;

    public OrderPaymentSucceededConsumer(IOrderService orderService, ILogger<OrderPaymentSucceededConsumer> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var ev = context.Message;

        _logger.LogInformation(
            "[OrderService] Nhận PaymentSucceededEvent — OrderId: {OrderId}, Amount: {Amount}",
            ev.OrderId, ev.Amount);

        try
        {
            // Chỉ chuyển sang Processing nếu đang ở New
            // (tức là thanh toán trước khi giao — BankTransfer/Momo)
            // COD: đơn đã Completed trước → no-op ở đây
            await _orderService.UpdateOrderStatusIfNewAsync(ev.OrderId, OrderStatus.Processing);
        }
        catch (Exception ex)
        {
            // Log và không throw → tránh requeue vô hạn
            _logger.LogWarning(
                "[OrderService] PaymentSucceededConsumer bỏ qua OrderId={OrderId}: {Message}",
                ev.OrderId, ex.Message);
        }
    }
}
