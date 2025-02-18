namespace Products.Infrastructure.Messaging
{
    using Products.Application.Interfaces;
    using RabbitMQ.Client;
    using System;
    using System.Text;
    using System.Text.Json;

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "product_created";

        public void PublishMessage<T>(T message)
        {
            var factory = new ConnectionFactory { HostName = _hostname };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

             channel.QueueDeclare(queue: _queueName,
                                  durable: false,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));


             channel.BasicPublish(
                exchange: "",
                routingKey: _queueName,
                false,
                basicProperties: null,
                body: body
             );

            Console.WriteLine($"[x] Sent {message}");
        }
    }
}
