using CommonService.Caching;
using CommonService.Common;
using CommonService.Exceptions;
using ProductService.Models;
using ProductService.Models.Request;
using ProductService.Models.Response;
using ProductService.Repository.Interface;
using ProductService.Services.Interface;
using ProductService.Specifications;

namespace ProductService.Services;

public class CategoryServiceImpl : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    private readonly ICacheService _cacheService;
    private const string CACHE_KEY_DROPDOWN = "categories:dropdown";

    public CategoryServiceImpl(ICategoryRepository categoryRepo, ICacheService cacheService)
    {
        _categoryRepo = categoryRepo;
        _cacheService = cacheService;
    }

    public async Task<ResultPaginationDto<CategoryResponse>> GetAllCategoriesAsync(CategoryFilterRequest request)
    {
        var spec = new CategoryFilterSpec(
            request.SearchTerm,
            request.SortBy,
            request.IsDescending,
            request.PageNumber,
            request.PageSize,
            request.IncludeDeleted);

        var countSpec = new CategoryFilterCountSpec(
            request.SearchTerm,
            request.IncludeDeleted);

        var totalItems = await _categoryRepo.CountAsync(countSpec);
        var categories = await _categoryRepo.ListAsync(spec);

        return new ResultPaginationDto<CategoryResponse>(
            categories.Select(MapToResponse).ToList(),
            request.PageNumber,
            request.PageSize,
            totalItems);
    }

    public async Task<List<CategoryDropdownResponse>> GetCategoryDropdownAsync()
    {
        // 1. Check in Cache
        var cached = await _cacheService.GetAsync<List<CategoryDropdownResponse>>(CACHE_KEY_DROPDOWN);
        if (cached != null)
        {
            return cached;
        }

        // 2. Not in Cache, fetch from DB
        var spec = new CategoryDropdownSpec();
        var categories = await _categoryRepo.ListAsync(spec);

        var response = categories.Select(c => new CategoryDropdownResponse
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        // 3. Save to Cache for 7 days
        await _cacheService.SetAsync(CACHE_KEY_DROPDOWN, response, TimeSpan.FromDays(7));

        return response;
    }

    public async Task<CategoryResponse> GetCategoryByIdAsync(Guid id)
    {
        var spec = new CategoryByIdSpec(id);
        var category = await _categoryRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy loại hàng hóa với ID: {id}");

        return MapToResponse(category);
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
    {
        // Kiểm tra tên danh mục trùng
        if (await _categoryRepo.NameExistsAsync(request.Name))
            throw new BadRequestException($"Loại hàng hóa '{request.Name}' đã tồn tại trong hệ thống.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);

        return MapToResponse(category);
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var spec = new CategoryByIdSpec(id);
        var category = await _categoryRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy loại hàng hóa với ID: {id}");

        // Kiểm tra tên trùng với danh mục khác
        if (!string.Equals(category.Name, request.Name, StringComparison.OrdinalIgnoreCase) &&
            await _categoryRepo.NameExistsAsync(request.Name, excludeId: id))
            throw new BadRequestException($"Loại hàng hóa '{request.Name}' đã được sử dụng.");

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.UpdatedAt = DateTime.UtcNow;

        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);

        return MapToResponse(category);
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        var spec = new CategoryByIdSpec(id);
        var category = await _categoryRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy loại hàng hóa với ID: {id}");

        var now = DateTime.UtcNow;

        // CASCADE soft delete: ẩn tất cả sản phẩm chưa bị xóa trong danh mục
        foreach (var product in category.Products.Where(p => !p.IsDeleted))
        {
            product.IsDeleted = true;
            product.UpdatedAt = now;
        }

        // Soft delete bản thân category
        category.IsDeleted = true;
        category.UpdatedAt = now;

        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);
    }

    public async Task RestoreCategoryAsync(Guid id)
    {
        // Phải dùng includeDeleted=true để tìm được cả entity đã xóa
        var spec = new CategoryByIdSpec(id, includeDeleted: true);
        var category = await _categoryRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy loại hàng hóa với ID: {id}");

        category.IsDeleted = false;
        category.UpdatedAt = DateTime.UtcNow;

        _categoryRepo.Update(category);
        await _categoryRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);
    }

    private static CategoryResponse MapToResponse(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        ProductCount = c.Products.Count(p => !p.IsDeleted),
        IsDeleted = c.IsDeleted,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    };
}
