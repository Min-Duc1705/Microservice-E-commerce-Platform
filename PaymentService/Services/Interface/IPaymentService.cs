using CommonService.Common;
using PaymentService.Models.Request;
using PaymentService.Models.Response;
using PaymentService.Utils.Enum;

namespace PaymentService.Services.Interface;

public interface IPaymentService
{
    /// <summary>Lấy danh sách giao dịch có filter + phân trang</summary>
    Task<ResultPaginationDto<PaymentTransactionResponse>> GetPagedPaymentsAsync(PaymentFilterRequest filter);

    /// <summary>Lấy chi tiết 1 giao dịch theo ID</summary>
    Task<PaymentTransactionResponse> GetPaymentByIdAsync(Guid id);

    /// <summary>Lấy giao dịch mới nhất của 1 đơn hàng</summary>
    Task<PaymentTransactionResponse?> GetLatestByOrderIdAsync(Guid orderId);

    /// <summary>Admin xác nhận thanh toán thủ công (COD / BankTransfer)</summary>
    Task<PaymentTransactionResponse> ConfirmManualPaymentAsync(Guid transactionId);

    /// <summary>Admin hoàn tiền 1 giao dịch</summary>
    Task<PaymentTransactionResponse> RefundAsync(Guid transactionId, decimal refundAmount, string? note);
}
