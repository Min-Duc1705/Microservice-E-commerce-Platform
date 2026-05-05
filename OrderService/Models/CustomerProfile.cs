using System;

namespace OrderService.Models;

/// <summary>
/// Bảng dữ liệu sao chép (Materialized View) từ CustomerService để tối ưu hóa truy vấn trong OrderService.
/// Nhờ bảng này, OrderService không cần gọi HTTP sang CustomerService khi cần lấy tên khách hàng.
/// </summary>
public class CustomerProfile
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Cập nhật khi nào (từ Event)
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}
