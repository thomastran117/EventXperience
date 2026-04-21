using System.Text;
using System.Text.Json;

using backend.main.configurations.environment;
using backend.main.publishers.interfaces;

using RabbitMQ.Client;

namespace backend.main.publishers.implementation
{
    public static class JsonOptions
    {
        public static readonly JsonSerializerOptions Default = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public sealed class Publisher : IPublisher, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly SemaphoreSlim _initializationLock = new(1, 1);
        private IConnection? _connection;
        private IChannel? _channel;
        private bool _disposed;

        public Publisher()
        {
            _factory = new ConnectionFactory
            {
                Uri = new Uri(EnvironmentSetting.RabbitConnection),
                AutomaticRecoveryEnabled = true
            };
        }

        public async Task PublishAsync<T>(string queue, T message)
        {
            var channel = await GetChannelAsync();
            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(message, JsonOptions.Default)
            );

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queue,
                body: body
            );
        }

        private async Task<IChannel> GetChannelAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(Publisher));

            if (_channel != null)
                return _channel;

            await _initializationLock.WaitAsync();
            try
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(Publisher));

                if (_channel != null)
                    return _channel;

                var connection = await _factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                try
                {
                    await DeclareQueuesAsync(channel);
                }
                catch
                {
                    await channel.CloseAsync();
                    await connection.CloseAsync();
                    throw;
                }

                _connection = connection;
                _channel = channel;
                return channel;
            }
            finally
            {
                _initializationLock.Release();
            }
        }

        private static async Task DeclareQueuesAsync(IChannel channel)
        {
            await channel.QueueDeclareAsync(
                queue: "eventxperience-email",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await channel.QueueDeclareAsync(
                queue: "clubpost-es-index-dlq",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await channel.QueueDeclareAsync(
                queue: "clubpost-es-index",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = "clubpost-es-index-dlq"
                }
            );

            await channel.QueueDeclareAsync(
                queue: "event-es-index-dlq",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await channel.QueueDeclareAsync(
                queue: "event-es-index",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = "",
                    ["x-dead-letter-routing-key"] = "event-es-index-dlq"
                }
            );
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_channel != null)
                await _channel.CloseAsync();

            if (_connection != null)
                await _connection.CloseAsync();

            _initializationLock.Dispose();
        }
    }
}
