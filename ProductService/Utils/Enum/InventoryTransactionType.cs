namespace ProductService.Utils.Enum;

public enum InventoryTransactionType
{
    /// <summary>Nhập hàng vào kho (mua từ nhà cung cấp)</summary>
    StockIn = 0,

    /// <summary>Xuất hàng khỏi kho do đơn hàng được duyệt (OrderCompleted)</summary>
    SaleOut = 1,

    /// <summary>Hoàn trả hàng vào kho do đơn hàng bị hủy (OrderCancelled)</summary>
    ReturnIn = 2,

    /// <summary>Điều chỉnh thủ công (kiểm kê, hao hụt...)</summary>
    Adjustment = 3,
}
