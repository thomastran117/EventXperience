using backend.main.configurations.environment;
using backend.main.utilities.implementation;

using RabbitMQ.Client;

namespace backend.main.consumers
{
    public sealed class ElasticsearchDlqMonitorService : BackgroundService
    {
        private const int AlertThreshold = 10;
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan ReconnectDelay = TimeSpan.FromSeconds(10);

        private static readonly string[] Queues =
        [
            "event-es-index-dlq",
            "clubpost-es-index-dlq"
        ];

        private readonly Dictionary<string, uint> _lastCounts = new(StringComparer.Ordinal);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            var factory = new ConnectionFactory
            {
                Uri = new Uri(EnvironmentSetting.RabbitConnection),
                AutomaticRecoveryEnabled = true
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                IConnection? connection = null;
                IChannel? channel = null;

                try
                {
                    connection = await factory.CreateConnectionAsync(stoppingToken);
                    channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                    Logger.Info("Elasticsearch DLQ monitor connected to RabbitMQ.");

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        foreach (var queue in Queues)
                        {
                            var result = await channel.QueueDeclareAsync(
                                queue: queue,
                                durable: true,
                                exclusive: false,
                                autoDelete: false,
                                cancellationToken: stoppingToken);

                            TrackQueueDepth(queue, result.MessageCount);
                        }

                        await Task.Delay(PollInterval, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "Elasticsearch DLQ monitor lost RabbitMQ connection. Reconnecting soon...");
                    await Task.Delay(ReconnectDelay, stoppingToken);
                }
                finally
                {
                    if (channel != null) { try { await channel.CloseAsync(); } catch { } }
                    if (connection != null) { try { await connection.CloseAsync(); } catch { } }
                }
            }
        }

        private void TrackQueueDepth(string queue, uint messageCount)
        {
            _lastCounts.TryGetValue(queue, out var previousCount);
            _lastCounts[queue] = messageCount;

            if (messageCount >= AlertThreshold)
            {
                if (previousCount != messageCount || previousCount < AlertThreshold)
                {
                    Logger.Error(
                        $"Elasticsearch DLQ '{queue}' contains {messageCount} messages. This exceeds the alert threshold of {AlertThreshold} and indicates persistent indexing failures.");
                }
                return;
            }

            if (messageCount > 0)
            {
                if (previousCount == 0 || previousCount >= AlertThreshold)
                {
                    Logger.Warn(
                        $"Elasticsearch DLQ '{queue}' contains {messageCount} message(s). Monitoring for further accumulation.");
                }
                return;
            }

            if (previousCount > 0)
            {
                Logger.Info($"Elasticsearch DLQ '{queue}' is back to zero messages.");
            }
        }
    }
}
