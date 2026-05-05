using AggregatorService.Models.Request;
using AggregatorService.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace AggregatorService.Services;

public class RolePageAggregatorService : IRolePageAggregatorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RolePageAggregatorService> _logger;

    public RolePageAggregatorService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<RolePageAggregatorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(int StatusCode, object? Data, string? Error, string Message)> GetRolePageAsync(RolePageRequest request, string token)
    {
        var client = _httpClientFactory.CreateClient();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var authBase = _configuration["Services:Auth"] ?? "http://localhost:5017";

        // Build query string cho Roles
        var roleQuery = HttpUtility.ParseQueryString(string.Empty);
        roleQuery["pageNumber"] = request.PageNumber.ToString();
        roleQuery["pageSize"] = request.PageSize.ToString();
        if (!string.IsNullOrEmpty(request.SearchTerm)) roleQuery["searchTerm"] = request.SearchTerm;
        if (request.IsActive.HasValue) roleQuery["isActive"] = request.IsActive.Value.ToString().ToLower();
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            roleQuery["sortBy"] = request.SortBy;
            roleQuery["isDescending"] = request.IsDescending.ToString().ToLower();
        }

        var roleUrl = $"{authBase}/api/v1/roles?{roleQuery}";
        var permUrl = $"{authBase}/api/v1/permissions/dropdown";

        _logger.LogInformation("Aggregating: Roles={RoleUrl}, Permissions={PermUrl}", roleUrl, permUrl);

        try
        {
            // Gọi song song 2 API
            var roleTask = client.GetAsync(roleUrl);
            var permTask = client.GetAsync(permUrl);

            await Task.WhenAll(roleTask, permTask);

            var roleResponse = await roleTask;
            var permResponse = await permTask;

            if (!roleResponse.IsSuccessStatusCode)
            {
                var err = await roleResponse.Content.ReadAsStringAsync();
                return ((int)roleResponse.StatusCode, null, err, "Lỗi khi tải danh sách chức vụ");
            }

            if (!permResponse.IsSuccessStatusCode)
            {
                var err = await permResponse.Content.ReadAsStringAsync();
                return ((int)permResponse.StatusCode, null, err, "Lỗi khi tải danh sách quyền hạn");
            }

            var roleJson = await roleResponse.Content.ReadAsStringAsync();
            var permJson = await permResponse.Content.ReadAsStringAsync();

            using var roleDoc = JsonDocument.Parse(roleJson);
            using var permDoc = JsonDocument.Parse(permJson);

            // Clone to avoid memory disposal issues
            var roleData = roleDoc.RootElement.TryGetProperty("data", out var rd)
                ? rd.Clone()
                : default;

            var permData = permDoc.RootElement.TryGetProperty("data", out var pd)
                ? pd.Clone()
                : default;

            var result = new
            {
                roles = roleData,
                permissions = permData
            };

            return (200, result, null, "Tải dữ liệu trang quản lý chức vụ thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi aggregate Roles Page");
            return (500, null, ex.Message, "Lỗi hệ thống khi tải dữ liệu");
        }
    }
}
