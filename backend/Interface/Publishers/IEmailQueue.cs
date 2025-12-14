namespace backend.Interfaces
{
    public interface IPublisher
    {
        Task PublishAsync<T>(string queue, T message);
    }
}
