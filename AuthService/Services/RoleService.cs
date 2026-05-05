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

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepo;
    private readonly IPermissionRepository _permissionRepo;
    private readonly ICacheService _cacheService;
    private const string CACHE_KEY_DROPDOWN = "roles:dropdown";

    public RoleService(IRoleRepository roleRepo, IPermissionRepository permissionRepo, ICacheService cacheService)
    {
        _roleRepo = roleRepo;
        _permissionRepo = permissionRepo;
        _cacheService = cacheService;
    }

    // ── GET ALL ──────────────────────────────────────────────────────────────

    public async Task<ResultPaginationDto<RoleResponse>> GetAllRolesAsync(RoleFilterRequest filter)
    {
        var spec = new RoleFilterSpec(filter.SearchTerm, filter.IsActive, filter.SortBy, filter.IsDescending, filter.PageNumber, filter.PageSize);
        var countSpec = new RoleFilterCountSpec(filter.SearchTerm, filter.IsActive);

        var roles = await _roleRepo.ListAsync(spec);
        var totalCount = await _roleRepo.CountAsync(countSpec);

        return new ResultPaginationDto<RoleResponse>(
            roles.Select(MapToResponse).ToList(),
            filter.PageNumber, filter.PageSize, totalCount);
    }

    // ── GET BY ID ────────────────────────────────────────────────────────────

    public async Task<RoleResponse> GetRoleByIdAsync(Guid id)
    {
        var spec = new RoleByIdSpec(id);
        var role = await _roleRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy Role với ID: {id}");

        return MapToResponse(role);
    }

    // ── CREATE ───────────────────────────────────────────────────────────────

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
    {
        var existing = await _roleRepo.GetByNameAsync(request.Name);
        if (existing != null)
            throw new BadRequestException($"Role '{request.Name}' đã tồn tại.");

        var permissions = new List<Permission>();
        foreach (var pid in request.PermissionIds)
        {
            var p = await _permissionRepo.GetEntityWithSpec(new PermissionByIdSpec(pid))
                ?? throw new BadRequestException($"Permission ID {pid} không tồn tại.");
            permissions.Add(p);
        }

        var role = new Role
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Permissions = permissions,
        };

        await _roleRepo.AddAsync(role);
        await _roleRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);

        return MapToResponse(role);
    }

    // ── UPDATE ───────────────────────────────────────────────────────────────

    public async Task<RoleResponse> UpdateRoleAsync(Guid id, UpdateRoleRequest request)
    {
        // Load with TRACKING so EF can detect collection changes for many-to-many
        var role = await _roleRepo.GetWithPermissionsAsync(id)
            ?? throw new NotFoundException($"Không tìm thấy Role với ID: {id}");

        var sameNameRole = await _roleRepo.GetByNameAsync(request.Name);
        if (sameNameRole != null && sameNameRole.Id != id)
            throw new BadRequestException($"Role '{request.Name}' đã tồn tại.");

        role.Name = request.Name;
        role.Description = request.Description;
        role.IsActive = request.IsActive;
        role.UpdatedAt = DateTime.UtcNow;

        if (request.PermissionIds != null)
        {
            if (!request.PermissionIds.Any())
                throw new BadRequestException("Phải có ít nhất 1 Permission.");

            var newIds = request.PermissionIds.ToHashSet();
            var currentIds = role.Permissions.Select(p => p.Id).ToHashSet();

            // Remove only permissions that are no longer needed
            var toRemove = role.Permissions.Where(p => !newIds.Contains(p.Id)).ToList();
            foreach (var p in toRemove)
                role.Permissions.Remove(p);

            // Add only permissions that are genuinely new (not already in collection)
            var toAddIds = newIds.Where(id => !currentIds.Contains(id)).ToList();
            if (toAddIds.Any())
            {
                var toAdd = await _permissionRepo.GetPermissionsByIdsAsync(toAddIds);
                if (toAdd.Count != toAddIds.Count)
                    throw new BadRequestException("Một hoặc nhiều Permission ID không tồn tại.");
                foreach (var p in toAdd)
                    role.Permissions.Add(p);
            }
        }

        _roleRepo.Update(role);
        await _roleRepo.SaveChangesAsync();

        // Invalidate dropdown cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);

        // Xóa Redis permission cache của TẤT CẢ users thuộc Role này
        // → Lần gọi API tiếp theo sẽ bị block ngay lập tức nếu không còn quyền
        await InvalidatePermissionCacheForRoleAsync(id);

        return MapToResponse(role);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    public async Task DeleteRoleAsync(Guid id)
    {
        var spec = new RoleByIdSpec(id);
        var role = await _roleRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy Role với ID: {id}");

        if (role.Users.Any())
            throw new BadRequestException(
                $"Không thể xóa Role '{role.Name}' vì có {role.Users.Count} user đang sử dụng.");

        _roleRepo.Delete(role);
        await _roleRepo.SaveChangesAsync();

        // Invalidate Cache
        await _cacheService.RemoveAsync(CACHE_KEY_DROPDOWN);
    }

    // ── DROPDOWN ─────────────────────────────────────────────────────────────

    public async Task<List<RoleDropdownDto>> GetDropdownAsync()
    {
        // 1. Check in Cache
        var cached = await _cacheService.GetAsync<List<RoleDropdownDto>>(CACHE_KEY_DROPDOWN);
        if (cached != null)
        {
            return cached;
        }

        // 2. Fetch from DB
        var roles = await _roleRepo.GetAllDropdownAsync();
        var response = roles.Select(r => new RoleDropdownDto { Id = r.Id, Name = r.Name! }).ToList();

        // 3. Save to Cache for 7 days
        await _cacheService.SetAsync(CACHE_KEY_DROPDOWN, response, TimeSpan.FromDays(7));

        return response;
    }

    // ── Mapping ──────────────────────────────────────────────────────────────

    private static RoleResponse MapToResponse(Role r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        IsActive = r.IsActive,
        UserCount = r.Users?.Count ?? 0,
        Permissions = r.Permissions?.Select(p => new PermissionResponse
        {
            Id = p.Id,
            Name = p.Name,
            ApiPath = p.ApiPath,
            Method = p.Method,
            Module = p.Module,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
        }).ToList() ?? new(),
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Xóa Redis permission cache ("perm:{email}") của tất cả users thuộc Role.
    /// Gọi sau khi admin thay đổi permissions của Role → hiệu lực ngay lập tức.
    /// </summary>
    private async Task InvalidatePermissionCacheForRoleAsync(Guid roleId)
    {
        var emails = await _roleRepo.GetUserEmailsByRoleIdAsync(roleId);
        if (emails.Count == 0) return;

        // Xóa song song tất cả key Redis cho nhanh
        var deleteTasks = emails.Select(email => _cacheService.RemoveAsync($"perm:{email}"));
        await Task.WhenAll(deleteTasks);
    }
}