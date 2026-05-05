using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using CommonService.Caching;
using CommonService.Common;
using CommonService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthService.Config;

/// <summary>
/// Kiểm tra quyền truy cập API dựa trên Permissions của Role — đọc từ Redis Cache.
/// Chạy tự động trước mỗi request (IAsyncAuthorizationFilter).
/// 
/// Phiên bản mới: không cần query DB, đọc từ Redis siêu nhanh (< 2ms).
/// </summary>
public class PermissionInterceptor : IAsyncAuthorizationFilter
{
    private readonly ICacheService _cache;
    private readonly ILogger<PermissionInterceptor> _logger;

    // Các path PUBLIC — không cần kiểm tra permission
    private static readonly List<string> WhitelistPaths = new()
    {
        "/api/v1/auth/login",
        "/api/v1/auth/register",
        "/api/v1/auth/refresh",
        "/api/v1/auth/account",
        "/api/v1/auth/logout",
        "/api/v1/admin/roles-page",
        "/swagger",
    };

    public PermissionInterceptor(ICacheService cache, ILogger<PermissionInterceptor> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";
        var method = context.HttpContext.Request.Method.ToUpper();

        // 1. Bỏ qua whitelist
        if (IsWhitelisted(path)) return;

        // 2. Phải đã authenticate (JWT hợp lệ)
        var userPrincipal = context.HttpContext.User;
        if (!userPrincipal.Identity?.IsAuthenticated ?? true) return;

        // 3. Lấy email từ Claims trong token
        var email = userPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            SetForbiddenResponse(context, "Không tìm thấy thông tin user trong token.");
            return;
        }

        // 4. Đọc permissions từ Redis (thay vì query DB)
        var permissions = await _cache.GetAsync<List<PermissionCacheDto>>($"perm:{email}");

        if (permissions == null || permissions.Count == 0)
        {
            _logger.LogWarning("Cache miss cho {Email} — Redis không có permissions hoặc đã hết hạn.", email);
            SetForbiddenResponse(context, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            return;
        }

        // 5. Kiểm tra permission: có permission khớp path + method không?
        var hasPermission = permissions.Any(p =>
            MatchPath(p.ApiPath, path) &&
            p.Method.Equals(method, StringComparison.OrdinalIgnoreCase));

        if (!hasPermission)
        {
            _logger.LogWarning("User {Email} bị từ chối: {Method} {Path}", email, method, path);
            SetForbiddenResponse(context, $"Bạn không có quyền truy cập: {method} {path}");
        }
    }

    /// <summary>So khớp path có wildcard {id}, {param}, etc.</summary>
    private static bool MatchPath(string pattern, string actualPath)
    {
        var regexPattern = "^" + Regex.Replace(
            pattern.ToLower(), @"\{[^}]+\}", "[^/]+") + "$";
        return Regex.IsMatch(actualPath.ToLower(), regexPattern);
    }

    private static bool IsWhitelisted(string path) =>
        WhitelistPaths.Any(w => path.StartsWith(w, StringComparison.OrdinalIgnoreCase));

    private static void SetForbiddenResponse(AuthorizationFilterContext context, string message)
    {
        context.Result = new JsonResult(new ApiResponse<object>
        {
            StatusCode = 403,
            Error = "Forbidden",
            Message = message,
            Data = null
        })
        { StatusCode = (int)HttpStatusCode.Forbidden };
    }
}
