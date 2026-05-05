using CommonService.Events;
using CustomerService.Models;
using CustomerService.Repository.Interface;
using MassTransit;

namespace CustomerService.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ICustomerRepository _customerRepo;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ICustomerRepository customerRepo, ILogger<UserRegisteredConsumer> logger)
    {
        _customerRepo = customerRepo;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation($"[CustomerService] Đã nhận event UserRegistered cho User: {msg.Email} (ID: {msg.UserId})");

        var existing = await _customerRepo.GetByEmailAsync(msg.Email);
        if (existing != null)
        {
            _logger.LogWarning($"[CustomerService] Customer với email {msg.Email} đã tồn tại! Bỏ qua tạo mới.");
            return;
        }

        var customer = new Customer
        {
            Id        = msg.UserId, // Dùng chung ID với AppUser bên AuthService
            FullName  = msg.Username,
            Email     = msg.Email,
            Phone     = "", // Cập nhật sau
            Address   = "", // Cập nhật sau
            CreatedAt = msg.RegisteredAt,
            UpdatedAt = msg.RegisteredAt
        };

        await _customerRepo.AddAsync(customer);
        await _customerRepo.SaveChangesAsync();

        _logger.LogInformation($"[CustomerService] Đã tạo thành công Customer profile cho {msg.Email} (ID: {msg.UserId})");
    }
}
