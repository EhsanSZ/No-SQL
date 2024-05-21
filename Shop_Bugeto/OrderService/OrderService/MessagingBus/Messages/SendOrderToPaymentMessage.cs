using OrderService.MessagingBus.SendMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.MessagingBus.Messages
{
    public class SendOrderToPaymentMessage:BaseMessage
    {
        public Guid OrderId { get; set; }
        public int Amount { get; set; }
    }
}
