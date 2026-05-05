using MassTransit;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Utils.Enum;
using CommonService.Events;

namespace PaymentService.Consumers;

/// <summary>
/// Hứng event OrderCreatedEvent từ OrderService.
/// Tạo bản ghi PaymentTransaction trạng thái Pending cho đơn hàng mới.
/// </summary>
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly PaymentDbContext _dbContext;

    public OrderCreatedConsumer(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var msg = context.Message;

        // Idempotency: tránh tạo trùng nếu Consumer nhận Event 2 lần
        var alreadyExists = _dbContext.PaymentTransactions
            .Any(t => t.OrderId == msg.OrderId);

        if (alreadyExists) return;

        // PaymentMethod trong Event là string ("COD", "BankTransfer", "VNPay", "Momo")
        Enum.TryParse<PaymentMethod>(msg.PaymentMethod, ignoreCase: true, out var method);

        var transaction = new PaymentTransaction
        {
            Id         = Guid.NewGuid(),
            OrderId    = msg.OrderId,
            CustomerId = msg.CustomerId,
            Amount     = msg.TotalAmount,
            Method     = method,
            Status     = PaymentStatus.Pending,
            CreatedAt  = DateTime.UtcNow,
        };

        _dbContext.PaymentTransactions.Add(transaction);
        await _dbContext.SaveChangesAsync();
    }
}

