using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CommonService.Caching;
using CommonService.Common;
using CommonService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CommonService.Filters;

/// <summary>
/// Authorization Filter kiểm tra Permission từ Redis Cache.
/// 
/// Gắn cùng với [RequiresPermission] Attribute lên Controller.
/// Không phụ thuộc DB — chỉ cần Redis, hoạt động trên mọi Microservice.
/// 
/// Flow:
///   1. Đọc [RequiresPermission(method, path)] từ metadata của endpoint
///   2. Lấy email từ JWT Claims
///   3. Đọc danh sách permissions từ Redis key "perm:{email}"
///   4. So khớp method + path → Allow hoặc 403
/// </summary>
public class RequiresPermissionFilter : IAsyncAuthorizationFilter
{
    private const string RedisKeyPrefix = "perm:";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. Tìm [RequiresPermission] attribute trên endpoint
        var permAttr = context.ActionDescriptor.EndpointMetadata
            .OfType<RequiresPermissionAttribute>()
            .FirstOrDefault();

        // Không có attribute → không cần check
        if (permAttr == null) return;

        // 2. Phải đã authenticate (JWT hợp lệ)
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            SetResponse(context, 401, "Unauthorized", "Bạn chưa đăng nhập hoặc token đã hết hạn.");
            return;
        }

        // 3. Lấy email từ Claims
        var email = user.FindFirst(ClaimTypes.Email)?.Value
                 ?? user.FindFirst("email")?.Value;
        
        if (string.IsNullOrEmpty(email))
        {
            SetResponse(context, 403, "Forbidden", "Không tìm thấy thông tin user trong token.");
            return;
        }

        // 4. Lấy permissions từ Redis
        var cache = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
        var permissions = await cache.GetAsync<List<PermissionCacheDto>>($"{RedisKeyPrefix}{email}");

        if (permissions == null || permissions.Count == 0)
        {
            SetResponse(context, 403, "Forbidden", "Phiên đăng nhập đã hết hạn hoặc không có quyền. Vui lòng đăng nhập lại.");
            return;
        }

        // 5. Kiểm tra có quyền khớp với attribute không
        var hasPermission = permissions.Any(p =>
            p.Method.Equals(permAttr.Method, StringComparison.OrdinalIgnoreCase) &&
            MatchPath(p.ApiPath, permAttr.ApiPath));

        if (!hasPermission)
        {
            SetResponse(context, 403, "Forbidden", $"Bạn không có quyền truy cập: {permAttr.Method} {permAttr.ApiPath}");
        }
    }

    /// <summary>
    /// So khớp path có wildcard, ví dụ: /api/v1/roles/{id} khớp với /api/v1/roles/abc123
    /// </summary>
    private static bool MatchPath(string pattern, string actualPath)
    {
        var regexPattern = "^" + Regex.Replace(
            pattern.ToLower(), @"\{[^}]+\}", "[^/]+") + "$";
        return Regex.IsMatch(actualPath.ToLower(), regexPattern);
    }

    private static void SetResponse(AuthorizationFilterContext context, int statusCode, string error, string message)
    {
        context.Result = new JsonResult(new ApiResponse<object>
        {
            StatusCode = statusCode,
            Error = error,
            Message = message,
            Data = null
        })
        { StatusCode = statusCode };
    }
}
