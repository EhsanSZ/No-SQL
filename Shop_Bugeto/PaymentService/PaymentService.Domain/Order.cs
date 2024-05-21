using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Domain
{
    public class Order
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public ICollection<Payment> Payments { get; set; }

    }
}
