namespace backend.main.publishers.interfaces
{
    public interface IPublisher
    {
        Task PublishAsync<T>(string queue, T message);
    }
}
