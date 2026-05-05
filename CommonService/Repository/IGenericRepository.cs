using CommonService.Specifications;

namespace CommonService.Repository;

/// <summary>
/// Generic Repository Interface — Kế thừa từ interface này để
/// tất cả các Service đều có sẵn các thao tác CRUD cơ bản + Specification.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    // ---- CRUD cơ bản ----
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    // ---- Specification Pattern ----
    /// <summary>Lấy 1 entity duy nhất theo Specification</summary>
    Task<T?> GetEntityWithSpec(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>Lấy danh sách entity theo Specification (có filter, sort, paging)</summary>
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

    /// <summary>Đếm tổng số bản ghi theo Specification (để tính tổng trang)</summary>
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
}
