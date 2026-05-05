using CommonService.Common;
using CommonService.Exceptions;
using MassTransit;
using PaymentService.Models;
using PaymentService.Models.Request;
using PaymentService.Models.Response;
using PaymentService.Repository.Interface;
using PaymentService.Services.Interface;
using PaymentService.Specifications;
using PaymentService.Utils.Enum;

namespace PaymentService.Services;

public class PaymentServiceImpl : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IPublishEndpoint _publishEndpoint;

    public PaymentServiceImpl(IPaymentRepository paymentRepo, IPublishEndpoint publishEndpoint)
    {
        _paymentRepo = paymentRepo;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<ResultPaginationDto<PaymentTransactionResponse>> GetPagedPaymentsAsync(PaymentFilterRequest filter)
    {
        // Chuẩn hóa DateTime sang UTC trước khi so sánh với PostgreSQL
        var fromDate = filter.FromDate.HasValue
            ? DateTime.SpecifyKind(filter.FromDate.Value, DateTimeKind.Utc) : (DateTime?)null;
        var toDate = filter.ToDate.HasValue
            ? DateTime.SpecifyKind(filter.ToDate.Value, DateTimeKind.Utc) : (DateTime?)null;

        var spec = new PaymentFilterSpec(
            filter.OrderId, filter.CustomerId, filter.Method, filter.Status,
            filter.SortBy, filter.IsDescending, filter.PageNumber, filter.PageSize,
            fromDate, toDate);

        var countSpec = new PaymentFilterCountSpec(
            filter.OrderId, filter.CustomerId, filter.Method, filter.Status,
            fromDate, toDate);

        var items      = await _paymentRepo.ListAsync(spec);
        var totalCount = await _paymentRepo.CountAsync(countSpec);

        return new ResultPaginationDto<PaymentTransactionResponse>(
            items.Select(MapToResponse).ToList(),
            filter.PageNumber, filter.PageSize, totalCount);
    }

    public async Task<PaymentTransactionResponse> GetPaymentByIdAsync(Guid id)
    {
        var spec        = new PaymentByIdSpec(id);
        var transaction = await _paymentRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy giao dịch với ID: {id}");

        return MapToResponse(transaction);
    }

    public async Task<PaymentTransactionResponse?> GetLatestByOrderIdAsync(Guid orderId)
    {
        var transaction = await _paymentRepo.GetLatestByOrderIdAsync(orderId);
        return transaction is null ? null : MapToResponse(transaction);
    }

    public async Task<PaymentTransactionResponse> ConfirmManualPaymentAsync(Guid transactionId)
    {
        var spec        = new PaymentByIdSpec(transactionId);
        var transaction = await _paymentRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy giao dịch với ID: {transactionId}");

        if (transaction.Status != PaymentStatus.Pending)
            throw new BadRequestException($"Chỉ có thể xác nhận giao dịch ở trạng thái Pending. Trạng thái hiện tại: {transaction.Status}");

        if (transaction.Method != PaymentMethod.COD && transaction.Method != PaymentMethod.BankTransfer)
            throw new BadRequestException("Chỉ có thể xác nhận thủ công cho COD hoặc BankTransfer.");

        transaction.Status    = PaymentStatus.Success;
        transaction.PaidAt    = DateTime.UtcNow;
        transaction.UpdatedAt = DateTime.UtcNow;

        _paymentRepo.Update(transaction);

        // Bắn sự kiện thanh toán thành công để OrderService/CustomerService cập nhật
        await _publishEndpoint.Publish(new CommonService.Events.PaymentSucceededEvent
        {
            OrderId = transaction.OrderId,
            CustomerId = transaction.CustomerId,
            Amount = transaction.Amount,
            PaymentMethod = transaction.Method.ToString(),
            TransactionId = transaction.Id.ToString(),
            ProcessedAt = transaction.PaidAt.Value
        });

        await _paymentRepo.SaveChangesAsync();

        return MapToResponse(transaction);
    }

    public async Task<PaymentTransactionResponse> RefundAsync(Guid transactionId, decimal refundAmount, string? note)
    {
        var spec        = new PaymentByIdSpec(transactionId);
        var transaction = await _paymentRepo.GetEntityWithSpec(spec)
            ?? throw new NotFoundException($"Không tìm thấy giao dịch với ID: {transactionId}");

        if (transaction.Status != PaymentStatus.Success)
            throw new BadRequestException("Chỉ có thể hoàn tiền giao dịch đã thành công.");

        if (refundAmount <= 0 || refundAmount > transaction.Amount - transaction.RefundedAmount)
            throw new BadRequestException($"Số tiền hoàn không hợp lệ. Tối đa có thể hoàn: {transaction.Amount - transaction.RefundedAmount:N0} VND.");

        transaction.RefundedAmount += refundAmount;
        transaction.RefundedAt      = DateTime.UtcNow;
        transaction.UpdatedAt       = DateTime.UtcNow;
        transaction.Status          = PaymentStatus.Refunded;
        if (!string.IsNullOrEmpty(note))
            transaction.FailureReason = note; // Tái sử dụng cột FailureReason cho ghi chú hoàn tiền

        _paymentRepo.Update(transaction);
        await _paymentRepo.SaveChangesAsync();

        return MapToResponse(transaction);
    }

    private static PaymentTransactionResponse MapToResponse(PaymentTransaction t) => new()
    {
        Id                  = t.Id,
        OrderId             = t.OrderId,
        CustomerId          = t.CustomerId,
        Method              = t.Method.ToString(),
        Status              = t.Status.ToString(),
        Amount              = t.Amount,
        GatewayTransactionId = t.GatewayTransactionId,
        PaymentUrl          = t.PaymentUrl,
        FailureReason       = t.FailureReason,
        RefundedAmount      = t.RefundedAmount,
        RefundedAt          = t.RefundedAt,
        PaidAt              = t.PaidAt,
        CreatedAt           = t.CreatedAt,
        UpdatedAt           = t.UpdatedAt,
    };
}
