# Hướng dẫn xử lý lỗi (Fallback): Gọi thẳng Database khi Redis sập

Phương án này (Fallback to Database) đảm bảo hệ thống vẫn hoạt động (resilient) ngay cả khi Redis gặp sự cố đứt kết nối hoặc sập nguồn. Khi Redis không phản hồi, hệ thống sẽ mở kết nối trực tiếp đến Database để truy vấn quyền của User.

> **⚠️ LƯU Ý VỀ KIẾN TRÚC MỚI (Anti-pattern trong Microservices)**
>
> Cách này có một nhược điểm chí mạng: Nó buộc các service khác (như `AggregatorService`, `CustomerService`) phải **biết về Database của AuthService**. Điều này phá vỡ nguyên tắc "Mỗi Microservice có một DB riêng và không được chọc vào DB của nhau". 
> Tuy nhiên, nếu bạn dùng chung 1 Database (Monolithic DB) cho toàn hệ thống thì cách này hoàn toàn khả thi và hiệu suất cao.

Dưới đây là hướng dẫn 2 bước chi tiết để thực hiện.

---

## Bước 1: Tạo Interface Fallback ở CommonService

Thay vì bắt `CommonService` phải cài đặt EntityFramework và reference thẳng tới `AuthDbContext` (sẽ gây vòng lặp rác mã), chúng ta dùng nguyên lý **Dependency Inversion (DI)**. 
`CommonService` sẽ định nghĩa một giao diện `IFallbackPermissionProvider`, và các service nào xài Filter sẽ tự implement giao diện này.

Tạo file `CommonService/Interface/IFallbackPermissionProvider.cs`:

```csharp
using CommonService.Models;

namespace CommonService.Interface;

/// <summary>
/// Interface cung cấp dữ liệu Fallback khi Redis sập.
/// Các Microservice (như AuthService, AggregatorService) sẽ implement interface này 
/// theo cách riêng của chúng (Query DB hoặc gọi HTTP API tùy ý).
/// </summary>
public interface IFallbackPermissionProvider
{
    Task<List<PermissionCacheDto>?> GetPermissionsFallbackAsync(string email);
}
```

---

## Bước 2: Bọc `try-catch` trong `RequiresPermissionFilter`

Cập nhật lại `RequiresPermissionFilter.cs` trong `CommonService` để bắt lỗi Redis và gọi Interface Fallback ở trên.

```csharp
using StackExchange.Redis; // Thêm thư viện để bắt lỗi Redis
using Microsoft.Extensions.Logging;
using CommonService.Interface;

public class RequiresPermissionFilter : IAsyncAuthorizationFilter
{
    // ... code cũ ...

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1-3. Code cũ kiểm tra attribute, authentication và trích xuất email...
        string email = "..."; 
        
        List<PermissionCacheDto>? permissions = null;
        var cache = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<RequiresPermissionFilter>>();

        // 4. THỬ ĐỌC TỪ REDIS VỚI TRY-CATCH
        try
        {
            permissions = await cache.GetAsync<List<PermissionCacheDto>>($"perm:{email}");
        }
        catch (RedisConnectionException ex) // Bắt đúng lỗi do Redis chết
        {
            logger.LogWarning(ex, "⚠️ Nháp: Redis đang sập! Kích hoạt chế độ Fallback vào Database cho user: {Email}", email);
            
            // XỬ LÝ FALLBACK TẠI ĐÂY
            // Lấy service IFallbackPermissionProvider từ DI Container
            var fallbackProvider = context.HttpContext.RequestServices.GetService<IFallbackPermissionProvider>();
            
            if (fallbackProvider != null)
            {
                permissions = await fallbackProvider.GetPermissionsFallbackAsync(email);
            }
            else
            {
                logger.LogError("Không tìm thấy IFallbackPermissionProvider để cứu giá khi Redis sập!");
            }
        }

        // 5. Kiểm tra quyền cuối cùng (dù lấy từ Redis hay DB, khúc này y hệt nhau)
        if (permissions == null || permissions.Count == 0)
        {
            SetResponse(context, 403, "Forbidden", "Phiên đăng nhập hết hạn hoặc Redis sập mà DB cũng trống.");
            return;
        }

        // 6. Loop qua list kiểm tra MatchPath...
    }
}
```

---

## Bước 3: Implement Fallback trực tiếp vào Database trên AuthService

Bây giờ về phía các microservices con (Ví dụ `AuthService` - vì thằng này chứa `AuthDbContext`).

Tạo class `DatabaseFallbackProvider` trong `AuthService/Services`:

```csharp
using CommonService.Interface;
using CommonService.Models;
using AuthService.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class DatabaseFallbackProvider : IFallbackPermissionProvider
{
    private readonly AuthDbContext _dbContext;

    public DatabaseFallbackProvider(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PermissionCacheDto>?> GetPermissionsFallbackAsync(string email)
    {
        // Dùng ef-core truy vấn thẳng từ DB (Chính là cách làm cũ của PermissionInterceptor!)
        var user = await _dbContext.Users
            .Include(u => u.Role)
                .ThenInclude(r => r.Permissions)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user?.Role?.Permissions == null) return null;

        return user.Role.Permissions.Select(p => new PermissionCacheDto
        {
            ApiPath = p.ApiPath,
            Method = p.Method,
            Module = p.Module
        }).ToList();
    }
}
```

Và ở `Program.cs` của **AuthService**, bạn chỉ việc tiêm nó vào:

```csharp
// Tiêm vào DI để RequiresPermissionFilter nhận diện
builder.Services.AddScoped<IFallbackPermissionProvider, DatabaseFallbackProvider>();
```

---

## Bước 4: Xử lý thế nào đối với AggregatorService? (Quan trọng)

`AggregatorService` hoàn toàn KHÔNG CÓ `AuthDbContext`. Nếu `RequiresPermissionFilter` được chạy tại `AggregatorService` mà Redis lại sập, nó sẽ làm gì?

Bạn có 2 lựa chọn cung cấp cho nó:

**Lựa chọn A: Phá lệ, cấp phép nối thẳng vào DB Auth**
Bạn copy chuỗi `ConnectionString` qua cho `AggregatorService`, cài thêm package EntityFramework, copy `AuthDbContext` qua rồi khai báo hệt bước 3. => **Rất dơ (Dirty code) vì 1 db 2 thằng chọc.**

**Lựa chọn B: Http Fallback (Chuẩn nhất)**
`AggregatorService` sẽ implement `IFallbackPermissionProvider` bằng cách gọi api qua HTTP.

Tạo class `HttpFallbackProvider.cs` trong `AggregatorService`:

```csharp
public class HttpFallbackProvider : IFallbackPermissionProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    
    public HttpFallbackProvider(IHttpClientFactory httpClientFactory){...}

    public async Task<List<PermissionCacheDto>?> GetPermissionsFallbackAsync(string email)
    {
        var client = _httpClientFactory.CreateClient("AuthService");
        
        // Gọi 1 API bí mật của AuthService (ví dụ GET /api/v1/auth/fallback-permissions?email=...)
        // API này của AuthService sẽ chọc vào DB lấy quyền và ném về
        var response = await client.GetFromJsonAsync<List<PermissionCacheDto>>($"/api/v1/internal/permissions?email={email}");
        
        return response;
    }
}
```

Khởi tạo ở `Program.cs` của Aggregator:
```csharp
builder.Services.AddScoped<IFallbackPermissionProvider, HttpFallbackProvider>();
```

## Tổng kết
Cơ chế **"Try-catch Redis > Interface Trung Gian > Implementation thực tiễn"** này là chuẩn chỉ nhất của SOLID, cho phép hệ thống "sạch" lỗi 500 khi Redis cháy và bạn có sự linh động trong việc chọn cách cứu (chọc DB trực tiếp hoặc chọc HTTP ngầm).
