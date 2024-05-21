using PaymentService.Infrastructure.MessagingBus.SendMessages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.MessagingBus.Messages
{
    public class PaymentIsDoneMessage:BaseMessage
    {
        public Guid OrderId { get; set; }

    }
}
