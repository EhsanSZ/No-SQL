using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.MessagingBus.SendMessages
{
    public interface IMessageBus
    {
        void SendMessage(BaseMessage message, string QueueName);
    }
}
