namespace CommonService.Events;

/// <summary>
/// Event published by AuthService when a new user registers.
/// CustomerService can consume this to create a default Customer profile.
/// </summary>
public class UserRegisteredEvent
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
