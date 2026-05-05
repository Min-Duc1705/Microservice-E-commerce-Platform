using CommonService.Annotations;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Models.Request;
using PaymentService.Services.Interface;
using PaymentService.Utils.Enum;
using CommonService.Filters;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Lấy danh sách giao dịch có filter + phân trang</summary>
    [HttpGet]
    [RequiresPermission("GET", "/api/v1/payments")]
    [ApiMessage("Lấy danh sách giao dịch thành công")]
    public async Task<IActionResult> GetAll([FromQuery] PaymentFilterRequest filter)
    {
        var result = await _paymentService.GetPagedPaymentsAsync(filter);
        return Ok(result);
    }

    /// <summary>Lấy chi tiết 1 giao dịch theo ID</summary>
    [HttpGet("{id:guid}")]
    [RequiresPermission("GET", "/api/v1/payments/{id}")]
    [ApiMessage("Lấy chi tiết giao dịch thành công")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _paymentService.GetPaymentByIdAsync(id);
        return Ok(result);
    }

    /// <summary>Lấy giao dịch mới nhất của 1 đơn hàng</summary>
    [HttpGet("by-order/{orderId:guid}")]
    [RequiresPermission("GET", "/api/v1/payments/by-order/{orderId}")]
    [ApiMessage("Lấy giao dịch theo đơn hàng thành công")]
    public async Task<IActionResult> GetByOrderId(Guid orderId)
    {
        var result = await _paymentService.GetLatestByOrderIdAsync(orderId);
        return Ok(result);
    }

    /// <summary>Admin xác nhận thanh toán thủ công (COD / BankTransfer)</summary>
    [HttpPut("{id:guid}/confirm")]
    [RequiresPermission("PUT", "/api/v1/payments/{id}/confirm")]
    [ApiMessage("Xác nhận thanh toán thành công")]
    public async Task<IActionResult> ConfirmManualPayment(Guid id)
    {
        var result = await _paymentService.ConfirmManualPaymentAsync(id);
        return Ok(result);
    }

    /// <summary>Admin hoàn tiền giao dịch</summary>
    [HttpPost("{id:guid}/refund")]
    [RequiresPermission("POST", "/api/v1/payments/{id}/refund")]
    [ApiMessage("Hoàn tiền thành công")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundRequest request)
    {
        var result = await _paymentService.RefundAsync(id, request.RefundAmount, request.Note);
        return Ok(result);
    }
}
