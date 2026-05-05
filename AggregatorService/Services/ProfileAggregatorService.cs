using AggregatorService.Models.Request;
using AggregatorService.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AggregatorService.Services;

public class ProfileAggregatorService : IProfileAggregatorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProfileAggregatorService> _logger;

    public ProfileAggregatorService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ProfileAggregatorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(bool IsSuccess, int StatusCode, object? Data, string? Error, string Message)> UpdateProfileAsync(
        UpdateProfileAggregatedRequest request, string token, string userId)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(token);

        var authBase = _configuration["Services:Auth"] ?? "http://localhost:5017";
        var customerBase = _configuration["Services:Customer"] ?? "http://localhost:5123";

        var authServiceUrl = $"{authBase}/api/v1/auth/email";
        var customerServiceUrl = $"{customerBase}/api/v1/customers/{userId}";

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        var authPayload = new StringContent(
            JsonSerializer.Serialize(new { email = request.Email, currentPassword = request.CurrentPassword, newPassword = request.NewPassword }, jsonOptions),
            Encoding.UTF8, "application/json");

        var customerPayload = new StringContent(
            JsonSerializer.Serialize(new { fullName = request.FullName, phone = request.Phone, address = request.Address }, jsonOptions),
            Encoding.UTF8, "application/json");

        try
        {
            var authTask = client.PutAsync(authServiceUrl, authPayload);
            var customerTask = client.PutAsync(customerServiceUrl, customerPayload);

            await Task.WhenAll(authTask, customerTask);

            var authResponse = await authTask;
            var customerResponse = await customerTask;

            if (!authResponse.IsSuccessStatusCode)
            {
                var errorContent = await authResponse.Content.ReadAsStringAsync();
                return (false, (int)authResponse.StatusCode, null, errorContent, "Cập nhật Email/Password thất bại");
            }

            if (!customerResponse.IsSuccessStatusCode)
            {
                var errorContent = await customerResponse.Content.ReadAsStringAsync();
                return (false, (int)customerResponse.StatusCode, null, errorContent, "Cập nhật thông tin khách hàng thất bại");
            }

            var rawCustomerJson = await customerResponse.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(rawCustomerJson);
            var root = jsonDoc.RootElement;
            var customerDataJson = root.TryGetProperty("data", out var dataProp) ? dataProp.GetRawText() : "{}";
            var customerData = JsonSerializer.Deserialize<object>(customerDataJson);

            return (true, (int)customerResponse.StatusCode, customerData, null, "Cập nhật Profile thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi cập nhật profile qua BFF.");
            return (false, 500, null, ex.Message, "Lỗi hệ thống khi cập nhật profile.");
        }
    }
}
