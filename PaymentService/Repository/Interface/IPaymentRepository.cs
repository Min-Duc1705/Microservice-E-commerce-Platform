using CommonService.Repository;
using PaymentService.Models;

namespace PaymentService.Repository.Interface;

/// <summary>
/// Kế thừa IGenericRepository để có CRUD + Specification.
/// Mở rộng thêm method đặc thù của Payment.
/// </summary>
public interface IPaymentRepository : IGenericRepository<PaymentTransaction>
{
    /// <summary>Kiểm tra đã có giao dịch thành công cho OrderId này chưa (tránh thanh toán 2 lần)</summary>
    Task<bool> HasSuccessfulPaymentAsync(Guid orderId);

    /// <summary>Lấy giao dịch mới nhất của 1 đơn hàng</summary>
    Task<PaymentTransaction?> GetLatestByOrderIdAsync(Guid orderId);
}
