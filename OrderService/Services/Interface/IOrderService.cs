using CommonService.Common;
using OrderService.Models.Request;
using OrderService.Models.Response;
using OrderService.Utils.Enum;

namespace OrderService.Services.Interface;

public interface IOrderService
{
    Task<ResultPaginationDto<OrderResponse>> GetPagedOrdersAsync(OrderFilterRequest filter);
    Task<OrderResponse> GetOrderByIdAsync(Guid id);
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, Guid? customerId);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus);

    /// <summary>
    /// Chỉ cập nhật status nếu đơn đang ở trạng thái New.
    /// Dùng bởi PaymentSucceededConsumer: BankTransfer/Momo xác nhận thanh toán trước → bắt đầu xử lý
    /// </summary>
    Task UpdateOrderStatusIfNewAsync(Guid orderId, OrderStatus newStatus);
}