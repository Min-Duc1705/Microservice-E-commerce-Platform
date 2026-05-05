using AuthService.Models;
using AuthService.Models.Request;
using AuthService.Models.Response;
using AuthService.Repository.Interface;
using AuthService.Services.Interface;
using CommonService.Caching;
using CommonService.Events;
using CommonService.Exceptions;
using CommonService.Models;
using MassTransit;

namespace AuthService.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly IAppUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ICacheService _cache;

    public AuthServiceImpl(
        IAppUserRepository userRepo,
        IRoleRepository roleRepo,
        ITokenService tokenService,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IPublishEndpoint publishEndpoint,
        ICacheService cache)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
        _tokenService = tokenService;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    // ── Login ────────────────────────────────────────────────────────────────

    public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Username)
            ?? throw new BadRequestException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new BadRequestException("Tài khoản đã bị khóa.");

        if (!user.IsEmailVerified)
            throw new BadRequestException($"Tài khoản chưa được xác thực email. Vui lòng kiểm tra hộp thư và xác thực.|{user.Email}");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new BadRequestException("Email hoặc mật khẩu không đúng.");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user);

        user.RefreshToken = refreshToken;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        SetRefreshTokenCookie(refreshToken);
        await PushPermissionsToCacheAsync(user); // ← lưu permissions vào Redis

        return BuildLoginResponse(accessToken, user);
    }

    // ── Register ─────────────────────────────────────────────────────────────

    public async Task<RegisterResponseDTO> RegisterAsync(RegisterRequestDTO request)
    {
        if (await _userRepo.EmailExistsAsync(request.Email))
            throw new BadRequestException($"Email '{request.Email}' đã tồn tại.");

        if (await _userRepo.UsernameExistsAsync(request.Username))
            throw new BadRequestException($"Username '{request.Username}' đã tồn tại.");

        // Gán Role mặc định = "USER" cho tài khoản mới
        var defaultRole = await _roleRepo.GetByNameAsync("USER")
            ?? throw new BadRequestException("Role 'USER' chưa tồn tại trong hệ thống. Vui lòng seed data trước.");

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            IsEmailVerified = false, // Chờ xác thực OTP
            RoleId = defaultRole.Id,
            CreatedAt = DateTime.UtcNow,
        };

        await _userRepo.AddAsync(user);

        // Sinh OTP và lưu Redis TTL 5 phút
        var otp = GenerateOtp();
        await _cache.SetAsync($"otp:register:{request.Email}", otp, TimeSpan.FromMinutes(5));

        // Publish event UserRegistered (CustomerService tạo profile)
        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            RegisteredAt = user.CreatedAt
        });

        // Publish event gửi OTP email qua NotificationService
        await _publishEndpoint.Publish(new OtpRequestedEvent
        {
            Email = user.Email,
            OtpCode = otp,
            OtpType = "REGISTER"
        });

        // 1 lần SaveChanges duy nhất: lưu AppUser + OutboxMessages cùng lúc
        await _userRepo.SaveChangesAsync();

        return new RegisterResponseDTO
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
        };
    }

    // ── Get Account ───────────────────────────────────────────────────────────

    public async Task<LoginResponseDTO> GetAccountAsync(string email)
    {
        var user = await _userRepo.GetByEmailAsync(email)
            ?? throw new NotFoundException($"Không tìm thấy user: {email}");

        return BuildLoginResponse(null, user);
    }

    // ── Refresh Token ─────────────────────────────────────────────────────────

    public async Task<LoginResponseDTO> RefreshTokenAsync(string refreshToken)
    {
        var principal = _tokenService.ValidateRefreshToken(refreshToken)
            ?? throw new BadRequestException("Refresh token không hợp lệ hoặc đã hết hạn.");

        var email = principal.Claims
            .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value
            ?? throw new BadRequestException("Refresh token không chứa email.");

        var user = await _userRepo.GetByRefreshTokenAsync(refreshToken)
            ?? throw new BadRequestException("Refresh token không khớp. Vui lòng đăng nhập lại.");

        if (user.Email != email)
            throw new BadRequestException("Refresh token không hợp lệ.");

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user);

        user.RefreshToken = newRefreshToken;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        SetRefreshTokenCookie(newRefreshToken);
        await PushPermissionsToCacheAsync(user); // ← cập nhật lại Redis

        return BuildLoginResponse(newAccessToken, user);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    public async Task LogoutAsync(string email)
    {
        var user = await _userRepo.GetByEmailAsync(email);
        if (user == null) return;

        user.RefreshToken = null;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        // Xóa cookie
        _httpContextAccessor.HttpContext?.Response.Cookies.Delete("refresh_token");

        // Xóa Redis permission cache
        await _cache.RemoveAsync($"perm:{email}");
    }

    // ── Update Email ────────────────────────────────────────────────────────

    public async Task UpdateEmailAsync(Guid userId, UpdateEmailRequestDTO request)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("Tài khoản không tồn tại.");

        // Nếu email mới giống email cũ thì bỏ qua
        if (user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase))
            return;

        // Nếu muốn đổi pass (optional)
        if (!string.IsNullOrEmpty(request.CurrentPassword) && !string.IsNullOrEmpty(request.NewPassword))
        {
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                throw new BadRequestException("Mật khẩu hiện tại không đúng.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        }

        // Kiểm tra trung email
        if (await _userRepo.EmailExistsAsync(request.Email))
            throw new BadRequestException($"Email '{request.Email}' đã được người khác sử dụng.");

        user.Email = request.Email;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);

        // Đã cập nhật xong trong bộ nhớ. Publish TRƯỚC SaveChanges để Outbox bắt được
        await _publishEndpoint.Publish(new UserEmailUpdatedEvent
        {
            UserId = user.Id,
            NewEmail = user.Email,
            UpdatedAt = user.UpdatedAt ?? DateTime.UtcNow
        });

        await _userRepo.SaveChangesAsync();
    }

    // ── Change Password (user tự đổi) ────────────────────────────────────────

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("Tài khoản không tồn tại.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Mật khẩu hiện tại không đúng.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var expireSeconds = int.Parse(_configuration["Jwt:RefreshTokenExpireSeconds"] ?? "604800");
        _httpContextAccessor.HttpContext!.Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,              // true khi deploy HTTPS
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromSeconds(expireSeconds)
        });
    }

    /// <summary>
    /// Push danh sách Permissions của User vào Redis Cache.
    /// Key: "perm:{email}", TTL = thời gian sống của AccessToken.
    /// Được gọi sau Login và RefreshToken để đảm bảo cache luôn fresh.
    /// </summary>
    private async Task PushPermissionsToCacheAsync(AppUser user)
    {
        var permissions = user.Role?.Permissions?.Select(p => new PermissionCacheDto
        {
            ApiPath = p.ApiPath,
            Method = p.Method,
            Module = p.Module
        }).ToList() ?? new List<PermissionCacheDto>();

        var expireSeconds = int.Parse(
            _configuration["Jwt:AccessTokenExpireSeconds"] ?? "900"); // 15 phút default

        await _cache.SetAsync(
            $"perm:{user.Email}",
            permissions,
            TimeSpan.FromSeconds(expireSeconds));
    }

    private static LoginResponseDTO BuildLoginResponse(string? accessToken, AppUser user) => new()
    {
        AccessToken = accessToken,
        User = new UserLoginDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role != null ? new UserLoginDto.RoleDto
            {
                Id = user.Role.Id,
                Name = user.Role.Name ?? "",
                Permissions = user.Role.Permissions?.Select(p => new UserLoginDto.PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    ApiPath = p.ApiPath,
                    Method = p.Method,
                    Module = p.Module,
                }).ToList() ?? new()
            } : null
        }
    };

    // ── OTP ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Xác minh OTP để kích hoạt tài khoản.
    /// </summary>
    public async Task VerifyEmailAsync(VerifyEmailRequest request)
    {
        // Lấy OTP từ Redis (Normalize lowercase để khớp với key lúc gửi)
        var cachedOtp = await _cache.GetAsync<string>($"otp:register:{request.Email.ToLowerInvariant()}");

        if (string.IsNullOrEmpty(cachedOtp))
            throw new BadRequestException("Mã OTP đã hết hạn hoặc không tồn tại. Vui lòng yêu cầu gửi lại.");

        if (cachedOtp != request.OtpCode)
            throw new BadRequestException("Mã OTP không đúng.");

        // Tìm user đang pending (IsEmailVerified = false)
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new NotFoundException("Không tìm thấy tài khoản đang chờ xác thực.");

        if (user.IsEmailVerified)
            throw new BadRequestException("Tài khoản này đã được xác thực trước đó.");

        // Kích hoạt tài khoản
        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        // Xóa OTP đã dùng
        await _cache.RemoveAsync($"otp:register:{request.Email.ToLowerInvariant()}");
    }

    /// <summary>
    /// Gửi OTP để đặt lại mật khẩu (Forgot Password).
    /// </summary>
    public async Task SendOtpResetPasswordAsync(SendOtpRequest request)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new NotFoundException("Không tìm thấy tài khoản với email này.");

        if (!user.IsEmailVerified)
            throw new BadRequestException("Tài khoản chưa được xác thực email.");

        var otp = GenerateOtp();

        await _cache.SetAsync($"otp:reset:{request.Email.ToLowerInvariant()}", otp, TimeSpan.FromMinutes(5));

        await _publishEndpoint.Publish(new OtpRequestedEvent
        {
            Email = request.Email,
            OtpCode = otp,
            OtpType = "RESET_PASSWORD"
        });
    }

    /// <summary>
    /// Đặt lại mật khẩu sau khi xác minh OTP.
    /// </summary>
    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var cachedOtp = await _cache.GetAsync<string>($"otp:reset:{request.Email}");

        if (string.IsNullOrEmpty(cachedOtp))
            throw new BadRequestException("Mã OTP đã hết hạn hoặc không tồn tại. Vui lòng yêu cầu gửi lại.");

        if (cachedOtp != request.OtpCode)
            throw new BadRequestException("Mã OTP không đúng.");

        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new NotFoundException("Không tìm thấy tài khoản.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        // Xóa OTP đã dùng
        await _cache.RemoveAsync($"otp:reset:{request.Email.ToLowerInvariant()}");
    }

    /// <summary>
    /// Gửi lại OTP.
    /// Nếu mã cũ vẫn còn hiệu lực trong Redis → gửi lại mã cũ (không tạo mới).
    /// Nếu đã hết hạn → tạo mã mới → lưu Redis → gửi email.
    /// </summary>
    public async Task ResendOtpAsync(SendOtpRequest request, string otpType)
    {
        var redisKey = otpType == "REGISTER"
            ? $"otp:register:{request.Email}"
            : $"otp:reset:{request.Email}";

        // Kiểm tra OTP còn tồn tại trong Redis không
        var existingOtp = await _cache.GetAsync<string>(redisKey);

        string otp;
        if (!string.IsNullOrEmpty(existingOtp))
        {
            // Mã cũ còn hạn → gửi lại đúng mã đó
            otp = existingOtp;
        }
        else
        {
            // Mã đã hết hạn → tạo mã mới + lưu Redis TTL 5 phút
            otp = GenerateOtp();
            await _cache.SetAsync(redisKey, otp, TimeSpan.FromMinutes(5));
        }

        // Publish event gửi email (dù mã cũ hay mới)
        await _publishEndpoint.Publish(new OtpRequestedEvent
        {
            Email = request.Email,
            OtpCode = otp,
            OtpType = otpType
        });

        // Cần SaveChanges để MassTransit Outbox flush message ra RabbitMQ
        // (Register flow hoạt động vì có SaveChanges sau Publish)
        await _userRepo.SaveChangesAsync();
    }

    private static string GenerateOtp()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
