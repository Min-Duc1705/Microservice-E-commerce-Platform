using CommonService.Repository;
using CustomerService.Data;
using CustomerService.Models;
using CustomerService.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Repository;

public class CustomerRepository : GenericRepository<CustomerDbContext, Customer>, ICustomerRepository
{
    public CustomerRepository(CustomerDbContext context) : base(context)
    {
    }

    public async Task<bool> PhoneExistsAsync(string phone, Guid? excludeId = null)
    {
        // IgnoreQueryFilters() để kiểm tra cả bản ghi đã soft delete
        var query = _context.Customers.IgnoreQueryFilters()
            .Where(c => c.Phone == phone);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var query = _context.Customers.IgnoreQueryFilters()
            .Where(c => c.Email.ToLower() == email.ToLower());

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email);
    }
}
