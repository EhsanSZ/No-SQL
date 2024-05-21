using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProductService.MessagingBus.Config;
using RabbitMQ.Client;
using System;
using System.Text;

namespace ProductService.MessagingBus
{
    public class RabbitMQMessageBus : IMessageBus
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;
        private IConnection _connection;

        public RabbitMQMessageBus(IOptions<RabbitMqConfiguration> rabbitMqOptions)
        {

            _hostname = rabbitMqOptions.Value.Hostname;
            _username = rabbitMqOptions.Value.UserName;
            _password = rabbitMqOptions.Value.Password;

            CreateRabbitMQConnection();

        }

        public void SendMessage(BaseMessage message, string exchange)
        {
            if (CneckRabbitMQConnection())
            {
                using (var channel = _connection.CreateModel())
                {
                    //channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                   
                    channel.ExchangeDeclare(exchange, ExchangeType.Fanout, true, false, null);

                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);
                    var Properties = channel.CreateBasicProperties();
                    Properties.Persistent = true;
                    channel.BasicPublish(exchange: exchange, routingKey: "", 
                        basicProperties: Properties, body: body);
                }
            }
        }
        private void CreateRabbitMQConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"can not create connection: {ex.Message}");
            }
        }

        private bool CneckRabbitMQConnection()
        {
            if (_connection != null)
            {
                return true;
            }
            CreateRabbitMQConnection();
            return _connection != null;
        }
    }

}
