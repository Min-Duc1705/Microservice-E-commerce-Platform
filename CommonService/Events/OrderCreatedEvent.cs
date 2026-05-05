using CommonService.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonService.Events
{
    public class OrderCreatedEvent : IEvent
    {
        public Guid OrderId { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public List<OrderItemInfo> Items { get; set; } = new List<OrderItemInfo>();
        public DateTime ProcessedAt { get; set; }
    }

    public class OrderItemInfo
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal UnitCost { get; set; } // Giá vốn, có thể để 0 nếu không cần thiết
    }
}
    

