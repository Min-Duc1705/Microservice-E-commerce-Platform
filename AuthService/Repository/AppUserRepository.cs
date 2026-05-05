using AuthService.Data;
using AuthService.Models;
using AuthService.Repository.Interface;
using CommonService.Repository;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repository;

public class AppUserRepository : GenericRepository<AuthDbContext, AppUser>, IAppUserRepository
{
    public AppUserRepository(AuthDbContext context) : base(context)
    {
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r!.Permissions)
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email.ToLower() ||
                u.Username.ToLower() == email.ToLower());
    }

    public async Task<AppUser?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .Include(u => u.Role)
                .ThenInclude(r => r!.Permissions)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username.ToLower() == username.ToLower());
    }
}