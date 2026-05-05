using CommonService.Events;
using CustomerService.Repository.Interface;
using MassTransit;

namespace CustomerService.Consumers;

public class UserEmailUpdatedConsumer : IConsumer<UserEmailUpdatedEvent>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly ILogger<UserEmailUpdatedConsumer> _logger;

    public UserEmailUpdatedConsumer(ICustomerRepository customerRepo, ILogger<UserEmailUpdatedConsumer> logger)
    {
        _customerRepo = customerRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserEmailUpdatedEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation($"[CustomerService] Đã nhận event UserEmailUpdated cho User ID: {msg.UserId} -> Email mới: {msg.NewEmail}");

        var customer = await _customerRepo.GetByIdAsync(msg.UserId);
        if (customer == null)
        {
            _logger.LogWarning($"[CustomerService] Không tìm thấy Customer ID {msg.UserId} để cập nhật Email.");
            return;
        }

        customer.Email = msg.NewEmail;
        customer.UpdatedAt = msg.UpdatedAt;

        _customerRepo.Update(customer);
        await _customerRepo.SaveChangesAsync();

        _logger.LogInformation($"[CustomerService] Đã cập nhật thành công Email cho Customer ID: {msg.UserId}");
    }
}
