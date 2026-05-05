using CustomerService.Utils.Enum;

namespace CustomerService.Models;

public class Customer
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    /// <summary>URL ảnh đại diện khách hàng (lưu từ MinIO). Nullable — không bắt buộc.</summary>
    public string? AvatarUrl { get; set; }

    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    /// <summary>Tổng chi tiêu tích lũy (cập nhật khi có đơn hoàn thành)</summary>
    public decimal TotalSpent { get; set; } = 0;

    /// <summary>Công nợ hiện tại (dương = đang nợ, 0 = không nợ)</summary>
    public decimal DebtAmount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Soft delete flag</summary>
    public bool IsDeleted { get; set; } = false;

    // ─── Navigation ───────────────────────────────────────────
    /// <summary>Toàn bộ lịch sử giao dịch công nợ của khách hàng này.</summary>
    public ICollection<DebtTransaction> DebtTransactions { get; set; } = new List<DebtTransaction>();
}

