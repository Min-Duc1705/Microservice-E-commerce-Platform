using CommonService.Specifications;
using OrderService.Models;

namespace OrderService.Specifications;

/// <summary>
/// Specification để lấy chi tiết 1 đơn hàng theo ID (bao gồm Items)
/// </summary>
public class OrderWithItemsSpec : BaseSpecification<Order>
{
    public OrderWithItemsSpec(Guid orderId) : base(o => o.Id == orderId)
    {
        AddInclude(o => o.Items);
    }
}
