using CommonService.Repository;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Repository.Interface;
using ProductService.Utils.Enum;

namespace ProductService.Repository;

public class ProductRepository : GenericRepository<ProductDbContext, Product>, IProductRepository
{
    public ProductRepository(ProductDbContext context) : base(context)
    {
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null)
    {
        // IgnoreQueryFilters() để kiểm tra cả bản ghi đã soft delete
        var query = _context.Products.IgnoreQueryFilters()
            .Where(p => p.SKU.ToLower() == sku.ToLower());

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task DecreaseStockAsync(Guid productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId)
            ?? throw new InvalidOperationException($"Không tìm thấy sản phẩm ID: {productId}");

        if (product.StockQuantity < quantity)
            throw new InvalidOperationException($"Sản phẩm '{product.Name}' không đủ tồn kho. Hiện còn: {product.StockQuantity}, cần: {quantity}");

        product.StockQuantity -= quantity;
        product.UpdatedAt = DateTime.UtcNow;

        // Tự động cập nhật trạng thái nếu hết hàng
        if (product.StockQuantity <= 0)
            product.Status = ProductStatus.OutOfStock;

        await _context.SaveChangesAsync();
    }

    public async Task IncreaseStockAsync(Guid productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId)
            ?? throw new InvalidOperationException($"Không tìm thấy sản phẩm ID: {productId}");

        product.StockQuantity += quantity;
        product.UpdatedAt = DateTime.UtcNow;

        // Nếu trước đó bị OutOfStock → chuyển lại Active khi có hàng
        if (product.Status == ProductStatus.OutOfStock && product.StockQuantity > 0)
            product.Status = ProductStatus.Active;

        await _context.SaveChangesAsync();
    }
}
