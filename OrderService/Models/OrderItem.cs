using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models
{
    public class OrderItem
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }

        /// <summary>Giá bán tại thời điểm đặt hàng (chốt, không đổi theo Product.Price)</summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Giá nhập (giá vốn) tại thời điểm đặt hàng.
        /// Chốt từ Product.CostPrice lúc tạo đơn — phục vụ báo cáo lợi nhuận chính xác.
        /// </summary>
        public decimal UnitCost { get; set; }

        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;

        [NotMapped]
        public decimal TotalCost => Quantity * UnitCost;

        /// <summary>Lợi nhuận gộp của dòng sản phẩm này trong đơn hàng.</summary>
        [NotMapped]
        public decimal Profit => TotalPrice - TotalCost;

        public Order? Order { get; set; }
    }
}

