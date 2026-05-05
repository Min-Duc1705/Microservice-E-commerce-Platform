namespace CommonService.Events;

/// <summary>
/// Event published by AuthService to request OTP email delivery.
/// NotificationService consumes this to send the OTP email.
/// </summary>
public class OtpRequestedEvent
{
    /// <summary>Email người nhận.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Mã OTP 6 số.</summary>
    public string OtpCode { get; set; } = string.Empty;

    /// <summary>"REGISTER" hoặc "RESET_PASSWORD"</summary>
    public string OtpType { get; set; } = string.Empty;
}
