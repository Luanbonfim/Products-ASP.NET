namespace Products.Application.Interfaces
{
    public interface IRabbitMqPublisher
    {
        void PublishMessage<T>(T message);
    }
}
