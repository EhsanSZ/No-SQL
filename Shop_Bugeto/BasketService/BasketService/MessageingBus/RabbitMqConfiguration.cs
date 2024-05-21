using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasketService.MessageingBus
{
    public class RabbitMqConfiguration
    {
        public string Hostname { get; set; }
        public string QueueName_BasketCheckout { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ExchangeName_UpdateProductName { get; set; }
        public string QueueName_GetMessageonUpdateProductName { get; set; }
    }
}
