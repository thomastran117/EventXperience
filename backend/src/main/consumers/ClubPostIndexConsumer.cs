using System.Text.Json;

using backend.main.configurations.environment;
using backend.main.dtos.messages;
using backend.main.models.documents;
using backend.main.publishers.implementation;
using backend.main.services.interfaces;
using backend.main.utilities.implementation;

using Polly;
using Polly.Retry;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace backend.main.consumers
{
    public class ClubPostIndexConsumer : BackgroundService
    {
        private const string MainQueue = "clubpost-es-index";
        private const string DlqQueue = "clubpost-es-index-dlq";

        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly ResiliencePipeline RetryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(500),
                ShouldHandle = new PredicateBuilder().Handle<Exception>()
            })
            .Build();

        public ClubPostIndexConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(EnvironmentSetting.RabbitConnection),
                AutomaticRecoveryEnabled = true
            };

            IConnection? connection = null;
            IChannel? channel = null;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    connection = await factory.CreateConnectionAsync(stoppingToken);
                    channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                    await channel.QueueDeclareAsync(
                        queue: DlqQueue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        cancellationToken: stoppingToken
                    );

                    await channel.QueueDeclareAsync(
                        queue: MainQueue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: new Dictionary<string, object?>
                        {
                            ["x-dead-letter-exchange"] = "",
                            ["x-dead-letter-routing-key"] = DlqQueue
                        },
                        cancellationToken: stoppingToken
                    );

                    await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += async (_, ea) => await HandleAsync(ea, channel);
                    await channel.BasicConsumeAsync(MainQueue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                    Logger.Info("ClubPostIndexConsumer started, listening on 'clubpost-es-index'.");

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "ClubPostIndexConsumer lost RabbitMQ connection. Reconnecting in 5s...");
                    await Task.Delay(5000, stoppingToken);
                }
                finally
                {
                    if (channel != null) { try { await channel.CloseAsync(); } catch { } }
                    if (connection != null) { try { await connection.CloseAsync(); } catch { } }
                }
            }
        }

        private async Task HandleAsync(BasicDeliverEventArgs ea, IChannel channel)
        {
            ClubPostIndexEvent? evt = null;
            try
            {
                evt = JsonSerializer.Deserialize<ClubPostIndexEvent>(ea.Body.Span, JsonOptions.Default);
                if (evt == null) throw new InvalidOperationException("Failed to deserialize ClubPostIndexEvent.");

                await RetryPipeline.ExecuteAsync(async ct =>
                {
                    using var scope = _scopeFactory.CreateScope();
                    var searchService = scope.ServiceProvider.GetRequiredService<IClubPostSearchService>();

                    if (evt.Operation == "delete")
                    {
                        await searchService.DeleteAsync(evt.PostId);
                    }
                    else
                    {
                        await searchService.IndexAsync(new ClubPostDocument
                        {
                            Id = evt.PostId,
                            ClubId = evt.ClubId ?? 0,
                            UserId = evt.UserId ?? 0,
                            Title = evt.Title ?? string.Empty,
                            Content = evt.Content ?? string.Empty,
                            PostType = evt.PostType ?? string.Empty,
                            LikesCount = evt.LikesCount ?? 0,
                            IsPinned = evt.IsPinned ?? false,
                            CreatedAt = evt.CreatedAt ?? DateTime.UtcNow,
                            UpdatedAt = evt.UpdatedAt ?? DateTime.UtcNow
                        });
                    }
                });

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                var postId = evt?.PostId.ToString() ?? "unknown";
                Logger.Warn(ex, $"ES indexing failed after retries for post {postId}. Sending to DLQ.");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
            }
        }
    }
}
