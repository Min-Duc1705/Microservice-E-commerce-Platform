using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonService.Interface
{
    public interface IEvent
    {
        DateTime ProcessedAt {get; set; }
    }
}
