using AuthService.Models.Request;
using AuthService.Models.Response;

namespace AuthService.Services.Interface;

public interface IAuthService
{
    Task<LoginResponseDTO>   LoginAsync(LoginRequestDTO request);
    Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO request);
    Task<LoginResponseDTO>   GetAccountAsync(string email);
    Task<LoginResponseDTO>   RefreshTokenAsync(string refreshToken);
    Task                     LogoutAsync(string email);
    
    // Thêm hàm cập nhật thông tin Account
    Task                     UpdateEmailAsync(Guid userId, UpdateEmailRequestDTO request);

    // Người dùng tự đổi mật khẩu (yêu cầu xác nhận mật khẩu cũ)
    Task                     ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

    // ── OTP ──────────────────────────────────────────────────────────────────
    /// <summary>Xác minh OTP để kích hoạt tài khoản sau đăng ký.</summary>
    Task VerifyEmailAsync(VerifyEmailRequest request);

    /// <summary>Gửi OTP để đặt lại mật khẩu.</summary>
    Task SendOtpResetPasswordAsync(SendOtpRequest request);

    /// <summary>Đặt lại mật khẩu mới sau khi xác minh OTP.</summary>
    Task ResetPasswordAsync(ResetPasswordRequest request);

    /// <summary>Gửi lại OTP: nếu mã cũ còn hiệu lực trong Redis thì gửi lại, nếu đã hết hạn thì tạo mã mới.</summary>
    Task ResendOtpAsync(SendOtpRequest request, string otpType);
}
