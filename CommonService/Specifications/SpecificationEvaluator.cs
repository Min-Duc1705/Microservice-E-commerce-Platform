using Microsoft.EntityFrameworkCore;

namespace CommonService.Specifications;

/// <summary>
/// Bộ đánh giá Specification — dịch ISpecification thành IQueryable.
/// Gắn các Criteria, Include, OrderBy, Paging vào query EF Core.
/// </summary>
public class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
    {
        var query = inputQuery;

        // 1. Áp dụng WHERE
        if (spec.Criteria != null)
        {
            query = query.Where(spec.Criteria);
        }

        // 2. Áp dụng INCLUDE (JOIN bảng liên quan)
        query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

        // 3. Áp dụng ORDER BY
        if (spec.OrderBy != null)
        {
            query = query.OrderBy(spec.OrderBy);
        }
        else if (spec.OrderByDescending != null)
        {
            query = query.OrderByDescending(spec.OrderByDescending);
        }

        // 4. Áp dụng PAGING (Skip & Take)
        if (spec.IsPagingEnabled)
        {
            query = query.Skip(spec.Skip).Take(spec.Take);
        }

        return query;
    }
}
