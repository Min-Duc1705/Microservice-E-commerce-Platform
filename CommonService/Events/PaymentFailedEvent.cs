using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonService.Events
{
    public class PaymentFailedEvent
    {
        public Guid OrderId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
    }
}
