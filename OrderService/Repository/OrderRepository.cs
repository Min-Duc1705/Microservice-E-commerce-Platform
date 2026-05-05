using CommonService.Repository;
using OrderService.Data;
using OrderService.Models;
using OrderService.Repository.Interface;

namespace OrderService.Repository;

/// <summary>
/// Kế thừa GenericRepository để có CRUD + Specification miễn phí.
/// Chỉ cần override hoặc thêm method nếu cần logic đặc biệt.
/// </summary>
public class OrderRepository : GenericRepository<OrderDbContext, Order>, IOrderRepository
{
    public OrderRepository(OrderDbContext context) : base(context)
    {
    }

    // Nếu cần method riêng nào đặc biệt cho Order thì thêm vào đây
    // Ví dụ:
    // public async Task<IEnumerable<Order>> GetOverdueOrdersAsync()
    // {
    //     return await _dbSet.Where(o => o.Status == OrderStatus.New && o.CreatedAt < DateTime.UtcNow.AddDays(-3)).ToListAsync();
    // }
}