using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonService.Events
{
    public class PaymentSucceededEvent
    {
        public Guid OrderId { get; set; }
        public Guid? CustomerId { get; set; }         // ← để CustomerService biết xóa nợ cho ai
        public decimal Amount { get; set; }           // ← để CustomerService biết xóa bao nhiêu
        public string PaymentMethod { get; set; } = string.Empty; // ← "COD" thì mới có nợ cần xóa
        public string TransactionId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}
