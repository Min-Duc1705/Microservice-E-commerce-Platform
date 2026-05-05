using CommonService.Repository;
using ProductService.Models;

namespace ProductService.Repository.Interface;

/// <summary>
/// Kế thừa IGenericRepository để có CRUD + Specification.
/// Mở rộng thêm các method đặc thù của Product.
/// </summary>
public interface IProductRepository : IGenericRepository<Product>
{
    /// <summary>Kiểm tra SKU đã tồn tại chưa (kể cả đã soft delete)</summary>
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null);

    /// <summary>Trừ tồn kho khi đơn hàng được xác nhận</summary>
    Task DecreaseStockAsync(Guid productId, int quantity);

    /// <summary>Hoàn trả tồn kho khi đơn hàng bị hủy</summary>
    Task IncreaseStockAsync(Guid productId, int quantity);
}
