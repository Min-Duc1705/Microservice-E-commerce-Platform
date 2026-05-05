namespace PaymentService.Models.Request;

public class RefundRequest
{
    public decimal RefundAmount { get; set; }
    public string? Note { get; set; }
}
