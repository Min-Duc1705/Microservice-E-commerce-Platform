# Hướng Dẫn: API Gateway & AuthService trong Microservices

> **Dành cho người mới — Giải thích từ GỐC đến NGỌN**

---

## 1. Tại Sao Cần API Gateway?

### Vấn đề khi KHÔNG có Gateway:

```
Frontend Angular
    │
    ├─── gọi http://localhost:5121  → OrderService
    ├─── gọi http://localhost:5122  → CustomerService
    ├─── gọi http://localhost:5123  → ProductService
    └─── gọi http://localhost:5124  → AuthService
```

**Hậu quả:**

- Frontend phải biết địa chỉ của **TẤT CẢ** services
- CORS phải cấu hình ở từng service
- Token phải validate ở từng service riêng lẻ
- Nếu đổi port/URL → phải sửa ở Frontend

### Giải pháp với API Gateway:

```
Frontend Angular
    │
    └─── gọi http://localhost:5000  → API Gateway (1 địa chỉ duy nhất)
                                            │
                                            ├─→  OrderService:5121
                                            ├─→  CustomerService:5122
                                            ├─→  ProductService:5123
                                            └─→  AuthService:5124
```

**Lợi ích:**

- Frontend chỉ biết **1 địa chỉ duy nhất**
- CORS, Auth, Rate Limiting → xử lý **1 chỗ**
- Services có thể thay đổi địa chỉ tự do, Gateway lo phần routing

---

## 2. Cài Đặt API Gateway với YARP (Yet Another Reverse Proxy)

Ocelot là thư viện Gateway phổ biến nhất trong hệ sinh thái .NET Microservices, cấu hình bằng một file JSON riêng biệt.

### Bước 1: Tạo Project mới

```bash
# Tạo project Web API trống
dotnet new web -n ApiGateway
cd ApiGateway

# Cài package Ocelot
dotnet add package Ocelot
```

### Bước 2: Tạo file `ocelot.json`

Ocelot dùng **file cấu hình riêng** tên `ocelot.json` (không gộp vào `appsettings.json`).

```json
{
  "Routes": [
    {
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE", "PATCH"],
      "DownstreamPathTemplate": "/api/auth/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5124 }],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": ""
      }
    },
    {
      "UpstreamPathTemplate": "/api/orders/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE", "PATCH"],
      "DownstreamPathTemplate": "/api/orders/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5121 }],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "UpstreamPathTemplate": "/api/customers/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE", "PATCH"],
      "DownstreamPathTemplate": "/api/customers/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5122 }],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    },
    {
      "UpstreamPathTemplate": "/api/products/{everything}",
      "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE", "PATCH"],
      "DownstreamPathTemplate": "/api/products/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "localhost", "Port": 5123 }],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer"
      }
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://localhost:5000"
  }
}
```

**Giải thích các key quan trọng:**

| Key                                   | Ý nghĩa                                                                               |
| ------------------------------------- | ------------------------------------------------------------------------------------- |
| `UpstreamPathTemplate`                | URL mà Frontend gọi đến Gateway                                                       |
| `DownstreamPathTemplate`              | URL Gateway sẽ forward đến Service                                                    |
| `DownstreamHostAndPorts`              | Địa chỉ thực của Service                                                              |
| `{everything}`                        | Giữ nguyên phần còn lại của URL (ví dụ `/api/orders/123` → forward `/api/orders/123`) |
| `AuthenticationProviderKey: "Bearer"` | Route này **bắt buộc phải có JWT Token** hợp lệ                                       |
| `AuthenticationProviderKey: ""`       | Route **công khai** (ví dụ đăng nhập, đăng ký — không cần token)                      |

### Bước 3: `Program.cs` của ApiGateway

```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Load file ocelot.json (tách riêng, không gộp vào appsettings.json)
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. Cấu hình JWT Validation tại Gateway
//    Ocelot sẽ dùng key "Bearer" này để xác thực các route có AuthenticationProviderKey = "Bearer"
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

// 3. CORS — chỉ cấu hình 1 lần ở Gateway, các service không cần
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 4. Đăng ký Ocelot
builder.Services.AddOcelot();

var app = builder.Build();

app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

// Ocelot middleware — PHẢI đặt cuối cùng, sau UseAuth*
await app.UseOcelot();

app.Run();
```

### Bước 4: Thêm `Jwt` config vào `appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Jwt": {
    "SecretKey": "SuperSecretKeyMinimum32CharactersLong!",
    "Issuer": "MicroserviceShop",
    "Audience": "MicroserviceShopClient"
  }
}
```

> **Lưu ý:** `SecretKey` ở đây PHẢI **y hệt** SecretKey trong `AuthService/appsettings.json`. Đây là "chìa khóa bí mật" chung để Gateway có thể xác minh token do AuthService tạo ra.

---

## 3. Xây Dựng AuthService

### AuthService làm những việc gì?

1. **Đăng ký** (`POST /api/auth/register`): Tạo tài khoản, hash mật khẩu, lưu vào DB
2. **Đăng nhập** (`POST /api/auth/login`): Xác minh mật khẩu, cấp JWT Token
3. **Làm mới token** (`POST /api/auth/refresh`): Cấp token mới khi token cũ hết hạn

### Cài Package cần thiết

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
```

### Model: `AppUser.cs`

```csharp
namespace AuthService.Models;

public class AppUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // ⚠️ KHÔNG BAO GIỜ lưu mật khẩu plaintext!
    // Luôn lưu dưới dạng đã hash bằng BCrypt
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer"; // "Customer" | "Admin" | "Staff"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Refresh Token để cấp lại Access Token khi hết hạn
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}
```

### DTOs

```csharp
// Request DTOs
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Response DTO
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Role { get; set; } = string.Empty;
}
```

### Service: `TokenService.cs`

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>
    /// Tạo JWT Access Token (ngắn hạn — thường 15 phút đến 1 giờ)
    /// </summary>
    public string GenerateAccessToken(AppUser user)
    {
        // Claims: Thông tin được "nhúng" vào trong token
        // Bất kỳ service nào đọc token đều biết được thông tin này
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),     // User ID
            new Claim(JwtRegisteredClaimNames.Email, user.Email),           // Email
            new Claim(ClaimTypes.Role, user.Role),                          // Role (Admin/Customer)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // JWT unique ID
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),  // Token hết hạn sau 1 giờ
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Tạo Refresh Token (dài hạn — thường 7-30 ngày)
    /// Dùng để xin cấp lại Access Token khi hết hạn
    /// </summary>
    public string GenerateRefreshToken()
    {
        // Random bytes → Base64 string (an toàn, không thể đoán được)
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

### Service: `AuthServiceImpl.cs`

```csharp
namespace AuthService.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly TokenService _tokenService;

    public AuthServiceImpl(IUserRepository userRepo, TokenService tokenService)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // 1. Kiểm tra email đã tồn tại chưa
        if (await _userRepo.EmailExistsAsync(request.Email))
            throw new BadRequestException("Email đã được sử dụng.");

        // 2. Hash mật khẩu bằng BCrypt (KHÔNG BAO GIỜ lưu plaintext!)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 3. Tạo user mới
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = "Customer" // Mặc định là Customer
        };

        // 4. Tạo Refresh Token và lưu vào DB
        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        // 5. Trả về Access Token + Refresh Token
        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = user.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Role = user.Role
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // 1. Tìm user theo email
        var user = await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new NotFoundException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new PermissionException("Tài khoản đã bị khóa.");

        // 2. Xác minh mật khẩu với BCrypt
        //    BCrypt.Verify() tự động so sánh plaintext với hash đã lưu
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new NotFoundException("Email hoặc mật khẩu không đúng.");

        // 3. Cập nhật Refresh Token mới
        user.RefreshToken = _tokenService.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = user.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Role = user.Role
        };
    }
}
```

### Controller: `AuthController.cs`

```csharp
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return StatusCode(201, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }
}
```

---

## 4. Luồng Hoạt Động Đầy Đủ

```
1. USER đăng nhập
   Angular ──POST /api/auth/login──→ Gateway ──→ AuthService
                                                      │
                               ←── { accessToken, refreshToken } ──┘
   Angular lưu token vào localStorage

2. USER gọi API cần xác thực
   Angular ──GET /api/orders──→ Gateway
      Header: Authorization: Bearer <accessToken>
                                │
              Gateway xác thực JWT (không cần gọi AuthService!)
                                │
              Nếu hợp lệ → forward đến OrderService
              Nếu sai/hết hạn → trả về 401 Unauthorized

3. Token hết hạn (sau 1 giờ)
   Angular ──POST /api/auth/refresh──→ AuthService (gửi refreshToken)
   AuthService kiểm tra refreshToken trong DB
   Nếu hợp lệ → cấp accessToken MỚI
   Nếu sai → trả về 401 → Angular chuyển về trang Login
```

**Tại sao Gateway xác thực được token mà không cần gọi AuthService?**

JWT Token được **ký** (sign) bằng một `SecretKey` bí mật. Bất kỳ ai có `SecretKey` đó đều có thể **xác minh** token mà không cần hỏi AuthService. Gateway và AuthService **chia sẻ cùng `SecretKey`** — đây chính là cơ chế.

---

## 5. Bảo Vệ Endpoint tại Từng Service (Lấy thông tin User từ Token)

Khi Gateway đã xác thực token và forward request đến OrderService, OrderService có thể đọc thông tin user từ token mà không cần query DB:

```csharp
// Trong OrdersController.cs
[HttpPost]
[Authorize] // Yêu cầu phải có token hợp lệ
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
{
    // Lấy UserId từ Claims trong token
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

    // Chỉ Admin mới được tạo đơn cho người khác
    if (userRole != "Admin" && request.CustomerId.ToString() != userId)
        throw new PermissionException("Bạn chỉ có thể tạo đơn hàng cho chính mình.");

    var order = await _orderService.CreateOrderAsync(request);
    return StatusCode(201, order);
}

// Chỉ Admin mới truy cập được
[HttpDelete("{id}")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> DeleteOrder(Guid id)
{
    await _orderService.DeleteOrderAsync(id);
    return Ok();
}
```

Để OrderService đọc được Claims từ token, cần thêm cấu hình JWT vào `Program.cs` của **từng service** (chỉ cần validate, không cần issuer SignIn):

```csharp
// OrderService/Program.cs — thêm phần JWT validation
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
            ValidateIssuer = false, // Gateway đã validate, service chỉ cần đọc Claims
            ValidateAudience = false
        };
    });
```

---

## 6. Cấu Trúc Thư Mục Đề Xuất

```
MicroserviceShop/
│
├── ApiGateway/            ← Cổng vào duy nhất (port 5000)
│   ├── Program.cs         ← Ocelot config + JWT validation + CORS
│   ├── ocelot.json        ← Route rules và downstream addresses
│   └── appsettings.json   ← Jwt SecretKey, Issuer, Audience
│
├── AuthService/           ← Quản lý xác thực (port 5124)
│   ├── Models/
│   │   └── AppUser.cs
│   ├── Models/Request/
│   │   ├── RegisterRequest.cs
│   │   └── LoginRequest.cs
│   ├── Models/Response/
│   │   └── AuthResponse.cs
│   ├── Services/
│   │   ├── TokenService.cs       ← Tạo JWT & Refresh Token
│   │   └── AuthServiceImpl.cs    ← Business Logic đăng ký/đăng nhập
│   ├── Controllers/
│   │   └── AuthController.cs
│   └── Program.cs
│
├── OrderService/   (port 5121) ← đã có
├── CustomerService/ (port 5122) ← đã có
└── CommonService/ ← Shared: ApiResponse, Exceptions, Specifications...
```

---

## 7. Checklist Tự Code Lại

### Bước 1: ApiGateway

- [ ] Tạo project `dotnet new web -n ApiGateway`
- [ ] Thêm `Ocelot` package
- [ ] Tạo file `ocelot.json` với Routes và DownstreamHostAndPorts
- [ ] Cấu hình CORS + JWT validation trong `Program.cs`
- [ ] Load `ocelot.json` trong `builder.Configuration`
- [ ] Thêm `AddOcelot()` và `await app.UseOcelot()`
- [ ] Test: gọi `GET localhost:5000/api/orders` xem có forward được không

### Bước 2: AuthService

- [ ] Tạo project `dotnet new webapi -n AuthService`
- [ ] Thêm packages: `JwtBearer`, `BCrypt.Net-Next`
- [ ] Tạo `AppUser` model
- [ ] Tạo `AuthDbContext` + migration
- [ ] Viết `TokenService` (GenerateAccessToken + GenerateRefreshToken)
- [ ] Viết `AuthServiceImpl` (Register + Login)
- [ ] Viết `AuthController`
- [ ] Cấu hình SecretKey trong `appsettings.json`
- [ ] Test: gọi `POST /api/auth/login` xem trả về token không

### Bước 3: Bảo vệ endpoints

- [ ] Thêm JWT validation vào `OrderService/Program.cs`
- [ ] Thêm `[Authorize]` vào các endpoint cần bảo vệ
- [ ] Test từ Gateway: gọi không có token → 401, có token → 200

> **Lưu ý bảo mật quan trọng:**
>
> - `Jwt:SecretKey` phải đủ dài (ít nhất 32 ký tự), ngẫu nhiên, và **không được commit lên Git**
> - Dùng `dotnet user-secrets` hoặc biến môi trường (môi trường production)
> - Access Token nên ngắn hạn (15-60 phút), Refresh Token dài hơn (7-30 ngày)
> - Refresh Token phải được lưu ở DB và xóa đi khi user đăng xuất
