using AuthService.Models;
using AuthService.Models.Request;
using AuthService.Models.Response;
using AuthService.Repository.Interface;
using AuthService.Services.Interface;
using AuthService.Specifications;
using CommonService.Common;
using CommonService.Exceptions;
using BC = BCrypt.Net.BCrypt;

namespace AuthService.Services;

public class UserService : IUserService
{
    private readonly IAppUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;

    public UserService(IAppUserRepository userRepo, IRoleRepository roleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    // ── GET ALL ──────────────────────────────────────────────────────────────

    public async Task<ResultPaginationDto<UserResponse>> GetAllUsersAsync(UserFilterRequest filter)
    {
        var spec = new UserFilterSpec(
            filter.SearchTerm, filter.IsActive, filter.RoleId,
            filter.SortBy, filter.IsDescending,
            filter.PageNumber, filter.PageSize);

        var countSpec = new UserFilterCountSpec(filter.SearchTerm, filter.IsActive, filter.RoleId);

        var users = await _userRepo.ListAsync(spec);
        var totalCount = await _userRepo.CountAsync(countSpec);

        return new ResultPaginationDto<UserResponse>(
            users.Select(MapToResponse).ToList(),
            filter.PageNumber, filter.PageSize, totalCount);
    }

    // ── GET BY ID ────────────────────────────────────────────────────────────

    public async Task<UserResponse> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepo.GetEntityWithSpec(new UserByIdSpec(id))
            ?? throw new NotFoundException($"Không tìm thấy user với ID: {id}");

        return MapToResponse(user);
    }

    // ── CREATE ───────────────────────────────────────────────────────────────

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        if (await _userRepo.EmailExistsAsync(request.Email))
            throw new BadRequestException($"Email '{request.Email}' đã tồn tại.");

        if (await _userRepo.UsernameExistsAsync(request.Username))
            throw new BadRequestException($"Username '{request.Username}' đã tồn tại.");

        if (request.RoleId.HasValue)
        {
            var roleExists = await _roleRepo.GetEntityWithSpec(new RoleByIdSpec(request.RoleId.Value));
            if (roleExists is null)
                throw new BadRequestException($"Role ID '{request.RoleId}' không tồn tại.");
        }

        var user = new AppUser
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BC.HashPassword(request.Password),
            RoleId = request.RoleId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        // Reload để lấy đầy đủ thông tin role
        return await GetUserByIdAsync(user.Id);
    }

    // ── UPDATE ───────────────────────────────────────────────────────────────

    public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepo.GetEntityWithSpec(new UserByIdSpec(id))
            ?? throw new NotFoundException($"Không tìm thấy user với ID: {id}");

        // Kiểm tra email trùng (ngoại trừ chính user này)
        if (!user.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)
            && await _userRepo.EmailExistsAsync(request.Email))
            throw new BadRequestException($"Email '{request.Email}' đã được sử dụng.");

        // Kiểm tra username trùng (ngoại trừ chính user này)
        if (!user.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)
            && await _userRepo.UsernameExistsAsync(request.Username))
            throw new BadRequestException($"Username '{request.Username}' đã được sử dụng.");

        if (request.RoleId.HasValue)
        {
            var roleExists = await _roleRepo.GetEntityWithSpec(new RoleByIdSpec(request.RoleId.Value));
            if (roleExists is null)
                throw new BadRequestException($"Role ID '{request.RoleId}' không tồn tại.");
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.RoleId = request.RoleId ?? user.RoleId;  // null → giữ nguyên role cũ
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        return MapToResponse(user);
    }

    // ── DELETE ───────────────────────────────────────────────────────────────

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userRepo.GetEntityWithSpec(new UserByIdSpec(id))
            ?? throw new NotFoundException($"Không tìm thấy user với ID: {id}");

        _userRepo.Delete(user);
        await _userRepo.SaveChangesAsync();
    }

    // ── RESET PASSWORD ───────────────────────────────────────────────────────

    public async Task ResetPasswordAsync(Guid id, ResetPasswordRequest request)
    {
        var user = await _userRepo.GetEntityWithSpec(new UserByIdSpec(id))
            ?? throw new NotFoundException($"Không tìm thấy user với ID: {id}");

        user.PasswordHash = BC.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }

    // ── MAP TO RESPONSE ──────────────────────────────────────────────────────

    private static UserResponse MapToResponse(AppUser user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Email = user.Email,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt,
        Role = user.Role is null ? null : new UserResponse.RoleDto
        {
            Id = user.Role.Id,
            Name = user.Role.Name,
        },
    };
}
