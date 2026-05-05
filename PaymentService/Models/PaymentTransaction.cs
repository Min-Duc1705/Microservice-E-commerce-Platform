using PaymentService.Utils.Enum;

namespace PaymentService.Models;

/// <summary>
/// Giao dịch thanh toán — mapping 1-1 với bảng PaymentTransactions trong DB.
/// Theo DB Analysis: 02.Database_Analysis.md Section 5.
/// </summary>
public class PaymentTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // ---------- Tham chiếu đến các service khác (không FK cứng) ----------
    /// <summary>Mã đơn hàng từ OrderService</summary>
    public Guid OrderId { get; set; }

    /// <summary>Mã khách hàng từ CustomerService (nullable — COD không bắt buộc)</summary>
    public Guid? CustomerId { get; set; }

    // ---------- Thông tin thanh toán ----------
    /// <summary>Phương thức: COD, BankTransfer, VNPay, Momo</summary>
    public PaymentMethod Method { get; set; }

    /// <summary>Trạng thái: Pending → Success / Failed / Refunded</summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Số tiền cần thanh toán</summary>
    public decimal Amount { get; set; }

    // ---------- Thông tin cổng thanh toán (VNPay/Momo) ----------
    /// <summary>Mã giao dịch phía cổng thanh toán trả về (để đối soát)</summary>
    public string? GatewayTransactionId { get; set; }

    /// <summary>URL thanh toán (deep link VNPay/Momo)</summary>
    public string? PaymentUrl { get; set; }

    /// <summary>Dữ liệu thô từ cổng thanh toán — lưu để debug khi cần</summary>
    public string? GatewayResponse { get; set; }

    // ---------- Lỗi / Hoàn tiền ----------
    /// <summary>Lý do thanh toán thất bại / ghi chú hoàn tiền</summary>
    public string? FailureReason { get; set; }

    /// <summary>Số tiền đã hoàn — mặc định 0</summary>
    public decimal RefundedAmount { get; set; } = 0;

    public DateTime? RefundedAt { get; set; }

    // ---------- Timestamps ----------
    /// <summary>Thời điểm thanh toán thành công — null nếu chưa thanh toán</summary>
    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
