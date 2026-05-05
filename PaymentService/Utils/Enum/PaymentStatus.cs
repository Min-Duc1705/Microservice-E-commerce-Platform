namespace PaymentService.Utils.Enum;

public enum PaymentStatus
{
    Pending   = 0,
    Success   = 1,   // đồng nghĩa với Succeeded
    Failed    = 2,
    Refunded  = 3,
    Cancelled = 4,
}