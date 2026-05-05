namespace CommonService.Events;

/// <summary>
/// Event published by AuthService when a user updates their email.
/// Other services (CustomerService, OrderService) should consume this to sync their redundant data.
/// </summary>
public class UserEmailUpdatedEvent
{
    public Guid UserId { get; set; }
    public string NewEmail { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
