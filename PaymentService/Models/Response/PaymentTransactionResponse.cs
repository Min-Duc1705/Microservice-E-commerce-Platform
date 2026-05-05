namespace PaymentService.Models.Response;

public class PaymentTransactionResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CustomerId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? PaymentUrl { get; set; }
    public string? FailureReason { get; set; }
    public decimal RefundedAmount { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
