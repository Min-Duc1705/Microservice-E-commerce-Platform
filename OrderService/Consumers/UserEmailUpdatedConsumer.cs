using CommonService.Events;
using MassTransit;
using OrderService.Data;

namespace OrderService.Consumers;

// Đặt tên riêng biệt để RabbitMQ tạo queue riêng, không tranh event với CustomerService
public class OrderUserEmailUpdatedConsumer : IConsumer<UserEmailUpdatedEvent>
{
    private readonly OrderDbContext _dbContext;
    private readonly ILogger<OrderUserEmailUpdatedConsumer> _logger;

    public OrderUserEmailUpdatedConsumer(OrderDbContext dbContext, ILogger<OrderUserEmailUpdatedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserEmailUpdatedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation($"[OrderService] Đã nhận event UserEmailUpdated. ID: {msg.UserId} -> {msg.NewEmail}");

        var profile = await _dbContext.CustomerProfiles.FindAsync(msg.UserId);
        if (profile == null)
        {
            _logger.LogWarning($"[OrderService] Profile ID {msg.UserId} không tồn tại để cập nhật Email.");
            return;
        }

        profile.Email = msg.NewEmail;
        profile.LastUpdatedAt = msg.UpdatedAt;

        _dbContext.CustomerProfiles.Update(profile);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation($"[OrderService] Đã cập nhật BẢN SAO Email cho User ID: {msg.UserId}");
    }
}
