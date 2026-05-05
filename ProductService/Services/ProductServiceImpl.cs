using CommonService.Common;
using CommonService.Exceptions;
using CommonService.Interface;
using ProductService.Models;
using ProductService.Models.Request;
using ProductService.Models.Response;
using ProductService.Repository.Interface;
using ProductService.Services.Interface;
using ProductService.Specifications;
using ProductService.Utils.Enum;

namespace ProductService.Services;

public class ProductServiceImpl : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IMediaService _mediaService;

    public ProductServiceImpl(
        IProductRepository productRepo,
        ICategoryRepository categoryRepo,
        IMediaService mediaService)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _mediaService = mediaService;
    }

    public async Task<ResultPaginationDto<ProductResponse>> GetPagedProductsAsync(ProductFilterRequest filter)
    {
        var spec = new ProductFilterSpec(
            filter.SearchTerm,
            filter.CategoryId,
            filter.Status,
            filter.SortBy,
            filter.IsDescending,
            filter.PageNumber,
            filter.PageSize,
            filter.FromDate,
            filter.ToDate,
            filter.IncludeDeleted);

        var countSpec = new ProductFilterCountSpec(
            filter.SearchTerm,
            filter.CategoryId,
            filter.Status,
            filter.FromDate,
            filter.ToDate,
            filter.IncludeDeleted);

        var products = await _productRepo.ListAsync(spec);
        var totalCount = await _productRepo.CountAsync(countSpec);

        var items = products.Select(MapToResponse).ToList();

        return new ResultPaginationDto<ProductResponse>(items, filter.PageNumber, filter.PageSize, totalCount);
    }

    public async Task<ProductResponse> GetProductByIdAsync(Guid id)
    {
        var spec = new ProductByIdSpec(id, includeDeleted: true);
        var product = await _productRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

        return MapToResponse(product);
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        // Kiểm tra danh mục tồn tại
        var categorySpec = new CategoryByIdSpec(request.CategoryId, includeDeleted: true);
        var category = await _categoryRepo.GetEntityWithSpec(categorySpec)
            ?? throw new NotFoundException($"Không tìm thấy loại hàng hóa với ID: {request.CategoryId}");

        // Xử lý SKU — tự động tạo nếu không nhập
        var sku = string.IsNullOrWhiteSpace(request.SKU)
            ? GenerateSku(request.Name)
            : request.SKU.Trim().ToUpper();

        // Kiểm tra SKU trùng
        if (await _productRepo.SkuExistsAsync(sku))
            throw new BadRequestException($"Mã SKU '{sku}' đã tồn tại trong hệ thống.");

        // Xác định trạng thái ban đầu
        var initialStatus = request.StockQuantity <= 0
            ? ProductStatus.OutOfStock
            : ProductStatus.Active;

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            SKU = sku,
            Description = request.Description,
            CategoryId = request.CategoryId,
            CostPrice = request.CostPrice,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Unit = request.Unit,
            LowStockThreshold = request.LowStockThreshold,
            Status = initialStatus,
            ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl)
                ? string.Empty
                : await _mediaService.CommitFileAsync(request.ThumbnailUrl, "products"),
            ImageUrls = request.ImageUrls?.Count > 0
                ? await _mediaService.CommitMultipleAsync(request.ImageUrls, "products")
                : [],
            CreatedAt = DateTime.UtcNow
        };

        await _productRepo.AddAsync(product);
        await _productRepo.SaveChangesAsync();

        // Gắn Category để map response
        product.Category = category;

        return MapToResponse(product);
    }

    public async Task<ProductResponse> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var spec = new ProductByIdSpec(id, includeDeleted: true);
        var product = await _productRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

        // Kiểm tra danh mục tồn tại
        var categorySpec = new CategoryByIdSpec(request.CategoryId, includeDeleted: true);
        var category = await _categoryRepo.GetEntityWithSpec(categorySpec)
            ?? throw new NotFoundException($"Không tìm thấy loại hàng hóa với ID: {request.CategoryId}");

        // Kiểm tra SKU trùng với sản phẩm khác
        var newSku = request.SKU.Trim().ToUpper();
        if (!string.Equals(product.SKU, newSku, StringComparison.OrdinalIgnoreCase) &&
            await _productRepo.SkuExistsAsync(newSku, excludeId: id))
            throw new BadRequestException($"Mã SKU '{newSku}' đã được dùng bởi sản phẩm khác.");

        product.Name = request.Name;
        product.SKU = newSku;
        product.Description = request.Description;
        product.CategoryId = request.CategoryId;
        product.Category = category;
        product.CostPrice = request.CostPrice;
        product.Price = request.Price;
        product.Unit = request.Unit;
        product.LowStockThreshold = request.LowStockThreshold;
        // Commit ảnh từ temp/ → products/ (nếu URL đã là products/ thì CommitFileAsync trả về nguyên)
        product.ThumbnailUrl = string.IsNullOrWhiteSpace(request.ThumbnailUrl)
            ? string.Empty
            : await _mediaService.CommitFileAsync(request.ThumbnailUrl, "products");
        product.ImageUrls = request.ImageUrls?.Count > 0
            ? await _mediaService.CommitMultipleAsync(request.ImageUrls, "products")
            : product.ImageUrls;
        product.UpdatedAt = DateTime.UtcNow;

        // Cập nhật tồn kho và tự điều chỉnh trạng thái nếu cần
        product.StockQuantity = request.StockQuantity;
        if (product.Status != ProductStatus.Discontinued)
        {
            product.Status = request.StockQuantity <= 0
                ? ProductStatus.OutOfStock
                : ProductStatus.Active;
        }

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();

        return MapToResponse(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var spec = new ProductByIdSpec(id, includeDeleted: true);
        var product = await _productRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

        // Soft delete — không xóa vật lý (theo Impl.md: giữ lịch sử bán hàng)
        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();
    }

    public async Task RestoreProductAsync(Guid id)
    {
        var spec = new ProductByIdSpec(id, includeDeleted: true);
        var product = await _productRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

        // Không cho phép restore nếu danh mục cha vẫn đang bị khóa
        if (product.Category is { IsDeleted: true })
            throw new BadRequestException(
                $"Không thể khôi phục sản phẩm khi danh mục \"{product.Category.Name}\" đang bị khóa. " +
                "Hãy khôi phục danh mục trước.");

        product.IsDeleted = false;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();
    }

    public async Task ToggleProductStatusAsync(Guid id, ProductStatus newStatus)
    {
        var spec = new ProductByIdSpec(id, includeDeleted: true);
        var product = await _productRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy sản phẩm với ID: {id}");

        if (product.Status == newStatus)
            throw new BadRequestException($"Sản phẩm đã ở trạng thái '{newStatus}'.");

        // Không cho phép chuyển sang Active nếu đang hết hàng
        if (newStatus == ProductStatus.Active && product.StockQuantity <= 0)
            throw new BadRequestException("Không thể kích hoạt sản phẩm khi tồn kho = 0.");

        product.Status = newStatus;
        product.UpdatedAt = DateTime.UtcNow;

        _productRepo.Update(product);
        await _productRepo.SaveChangesAsync();
    }

    // ======================================================
    // HELPER: Auto-generate SKU từ tên sản phẩm
    // Ví dụ: "Áo Sơ Mi Nam" → "ASM-XXXXXXXX"
    // ======================================================
    private static string GenerateSku(string productName)
    {
        var words = productName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w[0].ToString().ToUpper());

        var prefix = string.Concat(words).Length > 0
            ? string.Concat(words)
            : "PRD";

        var uniquePart = Guid.NewGuid().ToString("N")[..8].ToUpper();

        return $"{prefix}-{uniquePart}";
    }

    // ======================================================
    // HELPER: Map Product entity → ProductResponse
    // ======================================================
    private static ProductResponse MapToResponse(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        SKU = p.SKU,
        Description = p.Description,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.Name ?? string.Empty,
        CostPrice = p.CostPrice,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        Unit = p.Unit,
        LowStockThreshold = p.LowStockThreshold,
        IsLowStock = p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0,
        Status = p.Status.ToString(),
        ThumbnailUrl = p.ThumbnailUrl,
        ImageUrls = p.ImageUrls,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        IsDeleted = p.IsDeleted
    };
}
