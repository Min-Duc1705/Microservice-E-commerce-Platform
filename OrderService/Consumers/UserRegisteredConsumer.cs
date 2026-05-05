using CommonService.Events;
using MassTransit;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Consumers;

// Đặt tên riêng biệt để RabbitMQ tạo queue riêng, không tranh event với CustomerService
public class OrderUserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly OrderDbContext _dbContext;
    private readonly ILogger<OrderUserRegisteredConsumer> _logger;

    public OrderUserRegisteredConsumer(OrderDbContext dbContext, ILogger<OrderUserRegisteredConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation($"[OrderService] Đã nhận tin nhắn UserRegistered cho: {msg.Email} (ID: {msg.UserId})");

        // Kiểm tra xem đã có chưa (Idempotency)
        var existing = await _dbContext.CustomerProfiles.FindAsync(msg.UserId);
        if (existing != null)
        {
            _logger.LogWarning($"[OrderService] CustomerProfile {msg.Email} đã tồn tại! Bỏ qua tạo mới.");
            return;
        }

        // Tạo bản copy
        var profile = new CustomerProfile
        {
            Id = msg.UserId,
            FullName = msg.Username,
            Email = msg.Email,
            LastUpdatedAt = DateTime.UtcNow
        };

        _dbContext.CustomerProfiles.Add(profile);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"[OrderService] Đã LƯU BẢN SAO thành công cho {msg.Email}");
    }
}

