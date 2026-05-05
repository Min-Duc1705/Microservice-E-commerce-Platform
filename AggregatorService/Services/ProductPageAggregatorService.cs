using AggregatorService.Models.Request;
using AggregatorService.Services.Interfaces;
using System.Text.Json;
using System.Web;

namespace AggregatorService.Services;

public class ProductPageAggregatorService : IProductPageAggregatorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProductPageAggregatorService> _logger;

    public ProductPageAggregatorService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ProductPageAggregatorService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(int StatusCode, object? Data, string? Error, string Message)> GetProductPageAsync(ProductPageRequest request)
    {
        var client = _httpClientFactory.CreateClient();
        var productBase = _configuration["Services:Product"] ?? "http://localhost:5165";
        // Product và Category cùng nằm trong ProductService (port 5165)
        var categoryBase = productBase;

        // Build query string cho Products
        var productQuery = HttpUtility.ParseQueryString(string.Empty);
        productQuery["pageNumber"] = request.PageNumber.ToString();
        productQuery["pageSize"] = request.PageSize.ToString();
        if (!string.IsNullOrEmpty(request.SearchTerm)) productQuery["searchTerm"] = request.SearchTerm;
        if (!string.IsNullOrEmpty(request.CategoryId)) productQuery["categoryId"] = request.CategoryId;
        if (request.Status.HasValue) productQuery["status"] = request.Status.Value.ToString();
        if (!string.IsNullOrEmpty(request.SortBy))
        {
            productQuery["sortBy"] = request.SortBy;
            productQuery["isDescending"] = request.IsDescending.ToString().ToLower();
        }
        if (!string.IsNullOrEmpty(request.FromDate)) productQuery["fromDate"] = request.FromDate;
        if (!string.IsNullOrEmpty(request.ToDate)) productQuery["toDate"] = request.ToDate;
        productQuery["includeDeleted"] = request.IncludeDeleted.ToString().ToLower();

        var productUrl = $"{productBase}/api/v1/products?{productQuery}";
        // Gọi thẳng endpoint dropdown chuyên dụng (không phân trang, chỉ lấy Id và Name)
        var categoryUrl = $"{categoryBase}/api/v1/categories/dropdown";

        _logger.LogInformation("Aggregating: Products={ProductUrl}, Categories={CategoryUrl}", productUrl, categoryUrl);

        try
        {
            // Gọi song song 2 API
            var productTask = client.GetAsync(productUrl);
            var categoryTask = client.GetAsync(categoryUrl);

            await Task.WhenAll(productTask, categoryTask);

            var productResponse = await productTask;
            var categoryResponse = await categoryTask;

            if (!productResponse.IsSuccessStatusCode)
            {
                var err = await productResponse.Content.ReadAsStringAsync();
                return ((int)productResponse.StatusCode, null, err, "Lỗi khi tải danh sách sản phẩm");
            }

            if (!categoryResponse.IsSuccessStatusCode)
            {
                var err = await categoryResponse.Content.ReadAsStringAsync();
                return ((int)categoryResponse.StatusCode, null, err, "Lỗi khi tải danh sách danh mục");
            }

            var productJson = await productResponse.Content.ReadAsStringAsync();
            var categoryJson = await categoryResponse.Content.ReadAsStringAsync();

            // Parse và .Clone() NGAY LẬP TỨC trước khi JsonDocument bị dispose
            // Nếu không Clone, JsonElement trỏ vào bộ nhớ đã giải phóng → crash 500
            using var productDoc = JsonDocument.Parse(productJson);
            using var categoryDoc = JsonDocument.Parse(categoryJson);

            var productData = productDoc.RootElement.TryGetProperty("data", out var pd)
                ? pd.Clone()
                : default;

            var categoryData = categoryDoc.RootElement.TryGetProperty("data", out var cd)
                ? cd.Clone()
                : default;

            // Tách mảng danh mục ra khỏi wrapper pagination (nếu có)
            JsonElement categoryList;
            if (categoryData.ValueKind == JsonValueKind.Object && categoryData.TryGetProperty("result", out var catResult))
                categoryList = catResult.Clone();
            else
                categoryList = categoryData;

            var result = new
            {
                products = productData,
                categories = categoryList
            };

            return (200, result, null, "Tải dữ liệu trang sản phẩm thành công");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi aggregate Products Page");
            return (500, null, ex.Message, "Lỗi hệ thống khi tải dữ liệu");
        }
    }
}
