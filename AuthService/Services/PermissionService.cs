using AuthService.Models;
using AuthService.Models.Request;
using AuthService.Models.Response;
using AuthService.Repository.Interface;
using AuthService.Services.Interface;
using AuthService.Specifications;
using CommonService.Common;
using CommonService.Exceptions;

using CommonService.Caching;

namespace AuthService.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepo;
    private readonly ICacheService _cacheService;
    private const string CACHE_KEY_DROPDOWN = "permissions:dropdown";

    public PermissionService(IPermissionRepository permissionRepo, ICacheService cacheService)
    {
        _permissionRepo = permissionRepo;
        _cacheService = cacheService;
    }

    // ── GET ALL ──────────────────────────────────────────────────────────────

    public async Task<ResultPaginationDto<PermissionResponse>> GetAllPermissionsAsync(PermissionFilterRequest filter)
    {
        var spec      = new PermissionFilterSpec(filter.SearchTerm, filter.Module, filter.Method, filter.SortBy, filter.IsDescending, filter.PageNumber, filter.PageSize);
        var countSpec = new PermissionFilterCountSpec(filter.SearchTerm, filter.Module, filter.Method);

        var items      = await _permissionRepo.ListAsync(spec);
        var totalCount = await _permissionRepo.CountAsync(countSpec);

        return new ResultPaginationDto<PermissionResponse>(
            items.Select(MapToResponse).ToList(),
            filter.PageNumber, filter.PageSize, totalCount);
    }

    // ── GET BY ID ────────────────────────────────────────────────────────────

    public async Task<PermissionResponse> GetPermissionByIdAsync(Guid id)
    {
        var spec = new PermissionByIdSpec(id);
        var perm = await _permissionRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy Permission với ID: {id}");

        return MapToResponse(perm);
    }

    // ── CREATE ───────────────────────────────────────────────────────────────

    public async Task<PermissionResponse> CreatePermissionAsync(CreatePermissionRequest request)
    {
        var existing = await _permissionRepo.GetByPathAndMethodAsync(request.ApiPath, request.Method);
        if (existing != null)
            throw new BadRequestException($"Permission '{request.Method} {request.ApiPath}' đã tồn tại.");

        var permission = new Permission
        {
            Name      = request.Name,
            ApiPath   = request.ApiPath,
            Method    = request.Method.ToUpper(),
            Module    = request.Module,
            CreatedAt = DateTime.UtcNow,
        };

        await _permissionRepo.AddAsync(permission);
        await _permissionRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);

        return MapToResponse(permission);
    }

    // ── UPDATE ───────────────────────────────────────────────────────────────

    public async Task<PermissionResponse> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request)
    {
        var spec = new PermissionByIdSpec(id);
        var perm = await _permissionRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy Permission với ID: {id}");

        var conflict = await _permissionRepo.GetByPathAndMethodAsync(request.ApiPath, request.Method);
        if (conflict != null && conflict.Id != id)
            throw new BadRequestException($"Permission '{request.Method} {request.ApiPath}' đã tồn tại.");

        perm.Name      = request.Name;
        perm.ApiPath   = request.ApiPath;
        perm.Method    = request.Method.ToUpper();
        perm.Module    = request.Module;
        perm.UpdatedAt = DateTime.UtcNow;

        _permissionRepo.Update(perm);
        await _permissionRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);

        return MapToResponse(perm);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    public async Task DeletePermissionAsync(Guid id)
    {
        var spec = new PermissionByIdSpec(id);
        var perm = await _permissionRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy Permission với ID: {id}");

        _permissionRepo.Delete(perm);
        await _permissionRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);
    }

    // ── DROPDOWN ─────────────────────────────────────────────────────────────

    public async Task<List<PermissionResponse>> GetDropdownAsync()
    {
        // 1. Check in Cache
        var cached = await _cacheService.GetAsync<List<PermissionResponse>>(CACHE_KEY_DROPDOWN);
        if (cached != null)
        {
            return cached;
        }

        // 2. Fetch from DB
        var permissions = await _permissionRepo.GetAllDropdownAsync();
        var response = permissions.Select(MapToResponse).ToList();

        // 3. Save to Cache for 7 days
        await _cacheService.SetAsync(CACHE_KEY_DROPDOWN, response, TimeSpan.FromDays(7));

        return response;
    }

    // ── Mapping ──────────────────────────────────────────────────────────────

    private static PermissionResponse MapToResponse(Permission p) => new()
    {
        Id        = p.Id,
        Name      = p.Name,
        ApiPath   = p.ApiPath,
        Method    = p.Method,
        Module    = p.Module,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
    };
}