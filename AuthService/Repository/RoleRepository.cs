using AuthService.Data;
using AuthService.Models;
using AuthService.Repository.Interface;
using CommonService.Repository;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repository;

public class RoleRepository : GenericRepository<AuthDbContext, Role>, IRoleRepository
{
    public RoleRepository(AuthDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
    }

    public async Task<Role?> GetWithPermissionsAsync(Guid roleId)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<List<Role>> GetAllDropdownAsync()
    {
        return await _context.Roles
            .AsNoTracking()
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .Select(r => new Role { Id = r.Id, Name = r.Name })
            .ToListAsync();
    }

    public async Task<List<string>> GetUserEmailsByRoleIdAsync(Guid roleId)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.RoleId == roleId)
            .Select(u => u.Email)
            .ToListAsync();
    }
}