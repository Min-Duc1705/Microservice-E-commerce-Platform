using CommonService.Repository;
using ProductService.Models;

namespace ProductService.Repository.Interface;

/// <summary>
/// Kế thừa IGenericRepository để có CRUD + Specification.
/// </summary>
public interface ICategoryRepository : IGenericRepository<Category>
{
    /// <summary>Kiểm tra tên danh mục đã tồn tại chưa (kể cả đã soft delete)</summary>
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null);
}
