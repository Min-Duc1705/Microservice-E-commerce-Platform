using CommonService.Repository;
using OrderService.Models;

namespace OrderService.Repository.Interface;

/// <summary>
/// Kế thừa IGenericRepository để có sẵn CRUD + Specification,
/// chỉ thêm các method đặc thù riêng của Order nếu cần.
/// </summary>
public interface IOrderRepository : IGenericRepository<Order>
{
    // Thêm các method riêng của Order nếu cần trong tương lai
    // Ví dụ: Task<IEnumerable<Order>> GetOverdueOrdersAsync();
}