using System;

namespace PaymentService.Infrastructure.MessagingBus.SendMessages
{
    public class BaseMessage
    {
        public Guid MessageId { get; set; } = Guid.NewGuid();
        public DateTime Creationtime { get; set; } = DateTime.Now;
    }


}
