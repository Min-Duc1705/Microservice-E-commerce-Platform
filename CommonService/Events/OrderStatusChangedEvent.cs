using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonService.Events
{
    public class OrderStatusChangedEvent
    {
        public Guid OrderId { get; set; }
        public Guid? CustomerId { get; set; }         // ← để CustomerService biết update ai
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }      // ← để CustomerService cộng/trừ TotalSpent
        public string PaymentMethod { get; set; } = string.Empty; // ← để biết có phải COD không
        public List<OrderItemInfo> Items { get; set; } = new List<OrderItemInfo>();
        public DateTime ProcessedAt { get; set; }
    }
}
