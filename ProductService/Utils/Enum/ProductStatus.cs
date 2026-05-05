namespace ProductService.Utils.Enum;

public enum ProductStatus
{
    Active = 0,       // Đang bán
    Discontinued = 1, // Ngừng bán (bị khóa bởi admin)
    OutOfStock = 2    // Hết hàng (tự động khi StockQuantity <= 0)
}
