using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasketService.MessageingBus
{
    public interface IMessageBus
    {
        void SendMessage(BaseMessage message, string QueueName);
    }


    public class RabbitMQMessageBus : IMessageBus
    {
        private readonly string _hostname;
        private readonly string _password;
        private readonly string _username;
        private IConnection _connection;

        public RabbitMQMessageBus(IOptions<RabbitMqConfiguration> options)
        {
            _hostname = options.Value.Hostname;
            _username = options.Value.UserName;
            _password = options.Value.UserName;



        }

        public void SendMessage(BaseMessage message, string QueueName)
        {
            if (CheckRabbitMQConnection())
            {
                using (var channel = _connection.CreateModel())
                {
                    channel.QueueDeclare(queue: QueueName,
                        durable: true, exclusive: false, autoDelete: false,
                        arguments: null);
                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);
                    var Properties = channel.CreateBasicProperties();
                    Properties.Persistent = true;

                    channel.BasicPublish(exchange: "", routingKey: QueueName
                        , basicProperties: Properties, body: body);

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
                    Password = _password,
                };

                _connection = factory.CreateConnection();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"can not create connection: {ex.Message}");
            }
        }

        private bool CheckRabbitMQConnection()
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
