# Xác thực OTP qua Email (Flow B)

## Tổng quan

Tính năng xác thực OTP qua email được thêm vào `AuthService` theo **Luồng B**:

- Khi đăng ký: User cung cấp email → Hệ thống tạo tài khoản với `IsEmailVerified = false` → Gửi OTP 6 số → User nhập OTP → Tài khoản được kích hoạt.
- Khi quên mật khẩu: User nhập email → Nhận OTP → Nhập OTP + mật khẩu mới → Đổi thành công.
- OTP được lưu tạm vào **Redis với TTL 5 phút**, không cần bảng DB mới.
- Email được gửi qua **NotificationService** (microservice riêng) thông qua **RabbitMQ**.

---

## Kiến trúc

```
[Frontend]
    │
    ▼
[AuthService]
    ├── Sinh OTP 6 số
    ├── Lưu Redis: otp:register:{email} hoặc otp:reset:{email}  (TTL 5 phút)
    └── Publish OtpRequestedEvent → RabbitMQ
                                        │
                                        ▼
                               [NotificationService]  ← Consumer
                                        │
                                        └── Gửi Email (MailKit/SMTP)
```

---

## Các file thay đổi

### CommonService

#### [NEW] `Events/OtpRequestedEvent.cs`
Event được publish từ `AuthService` và consume bởi `NotificationService`.

```csharp
public class OtpRequestedEvent
{
    public string Email   { get; set; }  // Email người nhận
    public string OtpCode { get; set; }  // Mã OTP 6 số
    public string OtpType { get; set; }  // "REGISTER" | "RESET_PASSWORD"
}
```

---

### AuthService

#### [MODIFY] `Models/AppUser.cs`
Thêm field theo dõi trạng thái xác thực email:
```csharp
public bool IsEmailVerified { get; set; } = false;
```

#### [NEW] `Models/Request/SendOtpRequest.cs`
Request body cho API gửi OTP:
```csharp
public class SendOtpRequest { string Email; }
```

#### [NEW] `Models/Request/VerifyEmailRequest.cs`
Request body cho API xác minh OTP khi đăng ký:
```csharp
public class VerifyEmailRequest { string Email; string OtpCode; }
```

#### [MODIFY] `Models/Request/ResetPasswordRequest.cs`
Request body cho API đặt lại mật khẩu:
```csharp
public class ResetPasswordRequest { string Email; string OtpCode; string NewPassword; }
```

#### [MODIFY] `Services/Interface/IAuthService.cs`
Thêm 4 method signature mới:
```csharp
Task SendOtpRegisterAsync(SendOtpRequest request);
Task VerifyEmailAsync(VerifyEmailRequest request);
Task SendOtpResetPasswordAsync(SendOtpRequest request);
Task ResetPasswordAsync(ResetPasswordRequest request);
```

#### [MODIFY] `Services/AuthServiceImpl.cs`

**Thay đổi `LoginAsync`:** Chặn login nếu `IsEmailVerified = false`:
```csharp
if (!user.IsEmailVerified)
    throw new BadRequestException("Tài khoản chưa được xác thực email...");
```

**4 method OTP mới:**

| Method | Chức năng |
|---|---|
| `SendOtpRegisterAsync` | Kiểm tra email chưa tồn tại → sinh OTP → lưu Redis → publish event |
| `VerifyEmailAsync` | Lấy OTP từ Redis → so sánh → set `IsEmailVerified = true` → xóa OTP |
| `SendOtpResetPasswordAsync` | Tìm user → sinh OTP → lưu Redis → publish event |
| `ResetPasswordAsync` | Lấy OTP từ Redis → so sánh → BCrypt hash mật khẩu mới → xóa OTP |

**Private helper:**
```csharp
private static string GenerateOtp()
    => Random.Shared.Next(100000, 999999).ToString();
```

#### [MODIFY] `Controllers/AuthController.cs`
4 endpoint mới (tất cả `[AllowAnonymous]`):

| Method | Route | Chức năng |
|---|---|---|
| `POST` | `/api/v1/auth/send-otp-register` | Gửi OTP xác thực email đăng ký |
| `POST` | `/api/v1/auth/verify-email` | Xác minh OTP → kích hoạt tài khoản |
| `POST` | `/api/v1/auth/send-otp-reset` | Gửi OTP đặt lại mật khẩu |
| `POST` | `/api/v1/auth/reset-password` | Xác minh OTP → đổi mật khẩu mới |

---

## Database Migration

Do `IsEmailVerified` được thêm vào `AppUser`, cần chạy migration:

```bash
# Cài đặt dotnet-ef nếu chưa có
dotnet tool install --global dotnet-ef --version 8.0.*

# Trong thư mục AuthService
dotnet ef migrations add AddIsEmailVerified
dotnet ef database update
```

> ⚠️ Lưu ý: `dotnet ef database update` yêu cầu PostgreSQL đang chạy (`docker-compose up -d`)

---

## Redis Keys

| Key | TTL | Mục đích |
|---|---|---|
| `otp:register:{email}` | 5 phút | OTP đăng ký tài khoản |
| `otp:reset:{email}` | 5 phút | OTP đặt lại mật khẩu |

OTP bị xóa ngay lập tức sau khi dùng thành công (chỉ dùng được 1 lần).

---

## Các bước tiếp theo (TODO)

- [ ] Tạo `NotificationService` mới để consume `OtpRequestedEvent` và gửi email thật qua SMTP (MailKit).
- [ ] Cập nhật Frontend Angular: thêm luồng OTP vào trang Đăng ký và trang Quên mật khẩu.
- [ ] Cân nhắc thêm giới hạn số lần nhập OTP sai (Rate Limiting) để bảo mật.
