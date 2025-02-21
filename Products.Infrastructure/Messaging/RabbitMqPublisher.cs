using Microsoft.Extensions.Configuration;
using Products.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Products.Infrastructure.Messaging
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly string _hostname;
        private readonly string _queueName;
        private readonly string _username;
        private readonly string _password;

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _hostname = configuration["RabbitMq:HostName"];
            _queueName = configuration["RabbitMq:QueueName"];
            _username = configuration["RabbitMq:UserName"];
            _password = configuration["RabbitMq:Password"];
        }

        public async Task PublishMessage<T>(T message)
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                basicProperties: null,
                body: body
            );

            Console.WriteLine($"[x] Sent {message}");
        }
    }
}
