using OrderService.Utils.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;
        public decimal ShippingFee { get; set; } = 0;
        public string Note { get; set; } = string.Empty;
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        [NotMapped]
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);

        [NotMapped]
        public decimal TotalAmount => SubTotal + ShippingFee;
    }
}
