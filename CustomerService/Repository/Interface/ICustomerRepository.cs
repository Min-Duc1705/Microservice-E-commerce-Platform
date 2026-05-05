using CommonService.Repository;
using CustomerService.Models;

namespace CustomerService.Repository.Interface;

/// <summary>
/// Kế thừa IGenericRepository để có CRUD + Specification.
/// Có thể mở rộng thêm method đặc thù của Customer nếu cần.
/// </summary>
public interface ICustomerRepository : IGenericRepository<Customer>
{
    /// <summary>Kiểm tra SĐT đã tồn tại chưa (để tránh duplicate)</summary>
    Task<bool> PhoneExistsAsync(string phone, Guid? excludeId = null);

    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);

    /// <summary>Lấy thông tin Customer theo Email</summary>
    Task<Customer?> GetByEmailAsync(string email);
}
