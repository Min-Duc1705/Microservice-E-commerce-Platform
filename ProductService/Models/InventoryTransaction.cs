using ProductService.Utils.Enum;

namespace ProductService.Models;

/// <summary>
/// Thẻ kho — ghi lại mọi thay đổi số lượng tồn của từng sản phẩm.
/// Giúp đối soát kho chính xác và truy vết nguyên nhân biến động tồn kho.
/// </summary>
public class InventoryTransaction
{
    public Guid Id { get; set; }

    // ─── Liên kết ─────────────────────────────────────────────
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    // ─── Nghiệp vụ ────────────────────────────────────────────
    /// <summary>Loại biến động kho.</summary>
    public InventoryTransactionType Type { get; set; }

    /// <summary>
    /// Số lượng biến động (luôn DƯƠNG).
    /// Hệ thống sẽ tự ± vào Product.StockQuantity tuỳ theo Type.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Số lượng tồn kho SAU KHI thực hiện giao dịch này (snapshot để đối soát).
    /// </summary>
    public int StockAfter { get; set; }

    /// <summary>
    /// ID đơn hàng gây ra giao dịch này (nếu có).
    /// VD: SaleOut → OrderId; ReturnIn → OrderId bị huỷ.
    /// </summary>
    public Guid? ReferenceOrderId { get; set; }

    /// <summary>Ghi chú lý do điều chỉnh (áp dụng khi Type = Adjustment).</summary>
    public string Note { get; set; } = string.Empty;

    // ─── Audit ────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>ID Admin thực hiện giao dịch (cho Adjustment & StockIn).</summary>
    public Guid? CreatedByUserId { get; set; }
}
