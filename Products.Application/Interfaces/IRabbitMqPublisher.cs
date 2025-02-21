namespace Products.Application.Interfaces
{
    public interface IRabbitMqPublisher
    {
        Task PublishMessage<T>(T message);
    }
}
