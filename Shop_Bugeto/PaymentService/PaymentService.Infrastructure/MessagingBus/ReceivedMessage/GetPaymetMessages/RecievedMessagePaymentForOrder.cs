using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PaymentService.Application.Services;
using PaymentService.Infrastructure.MessagingBus.Configs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PaymentService.Infrastructure.MessagingBus.ReceivedMessage.GetPaymetMessages
{
    public class RecievedMessagePaymentForOrder : BackgroundService
    {
        private IModel _channel;
        private IConnection _connection;
        private readonly IPaymentService paymentService;
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _username;
        private readonly string _password;


        public RecievedMessagePaymentForOrder(IPaymentService paymentService
            , IOptions<RabbitMqConfiguration> rabbitMqOptions)
        {
            this.paymentService = paymentService;
            _hostname = rabbitMqOptions.Value.Hostname;
            _queueName = rabbitMqOptions.Value.QueueName_OrderSendToPayment;
            _username = rabbitMqOptions.Value.UserName;
            _password = rabbitMqOptions.Value.Password;


            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: _queueName,
                durable: true, exclusive: false,
                autoDelete: false, arguments: null);

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonConvert
                .DeserializeObject<MessagePaymentDto>(content);
                var resultHandeleMessage =
                HandleMessage(message.OrderId, message.Amount);

                if (resultHandeleMessage)
                    _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(_queueName, false, consumer);
            return Task.CompletedTask;
        }

        private bool HandleMessage(Guid OrderId, int Amount)
        {
            return paymentService.CreatePayment(OrderId, Amount);
        }

    }

    public class MessagePaymentDto
    {
        public Guid OrderId { get; set; }
        public int Amount { get; set; }
    }
}
