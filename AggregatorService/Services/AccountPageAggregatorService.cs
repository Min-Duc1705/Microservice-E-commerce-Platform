using AggregatorService.Models.Request;
using AggregatorService.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace AggregatorService.Services;

public class AccountPageAggregatorService : IAccountPageAggregatorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountPageAggregatorService> _logger;

    public AccountPageAggregatorService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AccountPageAggregatorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(int StatusCode, object? Data, string? Error, string Message)> GetAccountPageAsync(AccountPageRequest request, string token)
    {
        var client = _httpClientFactory.CreateClient();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var authBase = _configuration["Services:Auth"] ?? "http://localhost:5017";

        // Build query string cho Users
        var userQuery = HttpUtility.ParseQueryString(string.Empty);
        userQuery["pageNumber"] = request.PageNumber.ToString();
        userQuery["pageSize"] = request.PageSize.ToString();
        if (!string.IsNullOrEmpty(request.SearchTerm)) userQuery["searchTerm"] = request.SearchTerm;
        if (request.IsActive.HasValue) userQuery["isActive"] = request.IsActive.Value.ToString().ToLower();
        if (request.RoleId.HasValue) userQuery["roleId"] = request.RoleId.Value.ToString();
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            userQuery["sortBy"] = request.SortBy;
            userQuery["isDescending"] = request.IsDescending.ToString().ToLower();
        }

        var userUrl = $"{authBase}/api/v1/users?{userQuery}";
        var roleUrl = $"{authBase}/api/v1/roles/dropdown";

        _logger.LogInformation("Aggregating: Users={UserUrl}, Roles={RoleUrl}", userUrl, roleUrl);

        try
        {
            // Gọi song song 2 API
            var userTask = client.GetAsync(userUrl);
            var roleTask = client.GetAsync(roleUrl);

            await Task.WhenAll(userTask, roleTask);

            var userResponse = await userTask;
            var roleResponse = await roleTask;

            if (!userResponse.IsSuccessStatusCode)
            {
                var err = await userResponse.Content.ReadAsStringAsync();
                return ((int)userResponse.StatusCode, null, err, "Lỗi khi tải danh sách tài khoản");
            }

            if (!roleResponse.IsSuccessStatusCode)
            {
                var err = await roleResponse.Content.ReadAsStringAsync();
                return ((int)roleResponse.StatusCode, null, err, "Lỗi khi tải danh sách chức vụ");
            }

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var roleJson = await roleResponse.Content.ReadAsStringAsync();

            using var userDoc = JsonDocument.Parse(userJson);
            using var roleDoc = JsonDocument.Parse(roleJson);

            // Dùng .Clone() để tránh Memory disposal exception
            var userData = userDoc.RootElement.TryGetProperty("data", out var ud)
                ? ud.Clone()
                : default;

            var roleData = roleDoc.RootElement.TryGetProperty("data", out var rd)
                ? rd.Clone()
                : default;

            var result = new
            {
                users = userData,
                roles = roleData
            };

            return (200, result, null, "Tải dữ liệu trang quản lý tài khoản thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi aggregate Accounts Page");
            return (500, null, ex.Message, "Lỗi hệ thống khi tải dữ liệu");
        }
    }
}
