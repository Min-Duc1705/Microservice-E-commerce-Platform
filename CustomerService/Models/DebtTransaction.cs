using CustomerService.Utils.Enum;

namespace CustomerService.Models;

/// <summary>
/// Ghi nhận từng giao dịch công nợ của khách hàng.
/// Lịch sử này cho phép đối soát chính xác mọi khi số dư nợ thay đổi.
/// </summary>
public class DebtTransaction
{
    public Guid Id { get; set; }

    // ─── Liên kết ─────────────────────────────────────────────
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>
    /// Nếu giao dịch nợ phát sinh từ một đơn hàng cụ thể (VD: COD chưa thu tiền),
    /// ghi lại OrderId để truy vết nguồn gốc nợ.
    /// </summary>
    public Guid? ReferenceOrderId { get; set; }

    // ─── Nghiệp vụ ────────────────────────────────────────────
    /// <summary>Loại giao dịch: Ghi nợ mới (NewDebt) hoặc Trả nợ (Payment).</summary>
    public DebtTransactionType Type { get; set; }

    /// <summary>
    /// Số tiền của giao dịch (luôn là số DƯƠNG).
    /// Hệ thống sẽ tự ± vào DebtAmount của Customer tuỳ theo Type.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>Ghi chú do Admin nhập khi tạo phiếu thu (VD: "Khách trả tiền mặt 500k").</summary>
    public string Note { get; set; } = string.Empty;

    // ─── Audit ────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>ID tài khoản Admin đã thực hiện giao dịch này (để truy trách nhiệm).</summary>
    public Guid? CreatedBy { get; set; }
}
