using System.Linq.Expressions;

namespace CommonService.Specifications;

/// <summary>
/// Interface cốt lõi của Specification Pattern.
/// Đóng gói mọi điều kiện: WHERE, ORDER BY, INCLUDE, PAGING.
/// </summary>
public interface ISpecification<T>
{
    // WHERE clause - Điều kiện lọc
    Expression<Func<T, bool>>? Criteria { get; }

    // INCLUDE - Các bảng liên quan cần join
    List<Expression<Func<T, object>>> Includes { get; }

    // ORDER BY
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }

    // PAGING
    int Skip { get; }
    int Take { get; }
    bool IsPagingEnabled { get; }
}
