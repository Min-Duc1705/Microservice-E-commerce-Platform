using CommonService.Events;
using MassTransit;
using ProductService.Repository.Interface;

namespace ProductService.Consumers;

/// <summary>
/// Consumer nhận event OrderStatusChangedEvent từ OrderService.
/// Khi đơn hàng được xác nhận (Processing/Completed) → trừ tồn kho.
/// Khi đơn hàng bị hủy (Cancelled) → hoàn trả tồn kho.
/// </summary>
public class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedEvent>
{
    private readonly IProductRepository _productRepo;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;

    public OrderStatusChangedConsumer(IProductRepository productRepo, ILogger<OrderStatusChangedConsumer> logger)
    {
        _productRepo = productRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "[ProductService] Nhận OrderStatusChangedEvent — OrderId: {OrderId}, {OldStatus} → {NewStatus}",
            msg.OrderId, msg.OldStatus, msg.NewStatus);

        // Khi đơn chuyển sang "Processing" hoặc "Completed" → trừ tồn kho
        if (msg.NewStatus == "Processing" || msg.NewStatus == "Completed")
        {
            if (msg.OldStatus == "New")
            {
                foreach (var item in msg.Items)
                {
                    try
                    {
                        await _productRepo.DecreaseStockAsync(item.ProductId, item.Quantity);
                        _logger.LogInformation(
                            "[ProductService] Trừ {Qty} khỏi tồn kho ProductId={ProductId}",
                            item.Quantity, item.ProductId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "[ProductService] Lỗi khi trừ tồn kho ProductId={ProductId}", item.ProductId);
                    }
                }
            }
        }

        // Khi đơn bị hủy (Cancelled) → hoàn trả tồn kho
        // Chỉ hoàn kho khi trước đó là "Processing" (lúc đó đã trừ kho)
        // Nếu OldStatus là "New" thì kho CHƯ A bị trừ → không cần hoàn
        if (msg.NewStatus == "Cancelled" &&
            msg.OldStatus == "Processing")
        {
            foreach (var item in msg.Items)
            {
                try
                {
                    await _productRepo.IncreaseStockAsync(item.ProductId, item.Quantity);
                    _logger.LogInformation(
                        "[ProductService] Hoàn trả {Qty} vào tồn kho ProductId={ProductId}",
                        item.Quantity, item.ProductId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "[ProductService] Lỗi khi hoàn trả tồn kho ProductId={ProductId}", item.ProductId);
                }
            }
        }
    }
}
