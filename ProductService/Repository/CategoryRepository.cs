using CommonService.Repository;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Repository.Interface;

namespace ProductService.Repository;

public class CategoryRepository : GenericRepository<ProductDbContext, Category>, ICategoryRepository
{
    public CategoryRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null)
    {
        // IgnoreQueryFilters() để kiểm tra cả bản ghi đã soft delete
        var query = _context.Categories.IgnoreQueryFilters()
            .Where(c => c.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }
}
