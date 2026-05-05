using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Models.Response
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; } 
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty; 
        public string PaymentMethod { get; set; } = string.Empty;
        public List<OrderItemResponse> Items { get; set; } = new();
    }
    public class OrderItemResponse
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; } 
    }
}