using System.Text;
using System.Text.Json;

using backend.main.dtos.messages;
using backend.main.models.documents;
using backend.main.services.interfaces;
using backend.main.utilities.implementation;

using Confluent.Kafka;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace event_indexer;

public class EventIndexingWorker : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly EventIndexerOptions _options;
    private readonly IEventSearchService _searchService;

    public EventIndexingWorker(
        IOptions<EventIndexerOptions> options,
        IEventSearchService searchService)
    {
        _options = options.Value;
        _searchService = searchService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            AllowAutoCreateTopics = true
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            AllowAutoCreateTopics = true
        };

        using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        using var producer = new ProducerBuilder<Null, string>(producerConfig).Build();

        consumer.Subscribe(_options.Topic);
        Logger.Info(
            $"Event indexer subscribed to Kafka topic '{_options.Topic}' on '{_options.BootstrapServers}'."
        );

        await _searchService.EnsureIndexAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string>? result = null;

            try
            {
                result = consumer.Consume(stoppingToken);
                if (result?.Message == null)
                    continue;

                var eventType = GetHeaderValue(result.Message.Headers, "eventType")
                    ?? GetHeaderValue(result.Message.Headers, "type");

                await ProcessWithRetryAsync(result.Message.Value, eventType, stoppingToken);
                consumer.Commit(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex) when (result?.Message != null)
            {
                var eventType = GetHeaderValue(result.Message.Headers, "eventType")
                    ?? GetHeaderValue(result.Message.Headers, "type");

                Logger.Warn(
                    ex,
                    $"Event indexing failed for Kafka offset {result.TopicPartitionOffset}. Sending to DLQ."
                );

                await PublishDlqAsync(producer, result, eventType, ex, stoppingToken);
                consumer.Commit(result);
            }
        }

        producer.Flush(stoppingToken);
        consumer.Close();
    }

    private async Task ProcessWithRetryAsync(
        string payload,
        string? eventType,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await ProcessMessageAsync(payload, eventType, cancellationToken);
                return;
            }
            catch when (attempt < maxAttempts)
            {
                var delay = TimeSpan.FromMilliseconds(250 * attempt);
                await Task.Delay(delay, cancellationToken);
            }
        }

        await ProcessMessageAsync(payload, eventType, cancellationToken);
    }

    private async Task ProcessMessageAsync(
        string payload,
        string? eventType,
        CancellationToken cancellationToken)
    {
        switch (eventType)
        {
            case "upsert":
            {
                var document = JsonSerializer.Deserialize<EventDocument>(payload, JsonOptions)
                    ?? throw new InvalidOperationException(
                        "Kafka payload could not be deserialized into EventDocument."
                    );
                await _searchService.IndexAsync(document, cancellationToken);
                return;
            }

            case "delete":
            {
                var deletePayload = JsonSerializer.Deserialize<EventSearchDeletePayload>(
                    payload,
                    JsonOptions
                ) ?? throw new InvalidOperationException(
                    "Kafka payload could not be deserialized into EventSearchDeletePayload."
                );
                await _searchService.DeleteAsync(deletePayload.EventId, cancellationToken);
                return;
            }

            default:
                throw new InvalidOperationException(
                    $"Unsupported or missing event type header: '{eventType ?? "<null>"}'."
                );
        }
    }

    private async Task PublishDlqAsync(
        IProducer<Null, string> producer,
        ConsumeResult<string, string> result,
        string? eventType,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var dlq = new EventIndexerDlqRecord
        {
            SourceTopic = result.Topic,
            Partition = result.Partition.Value,
            Offset = result.Offset.Value,
            Key = result.Message.Key,
            EventType = eventType,
            Value = result.Message.Value,
            Error = exception.ToString()
        };

        await producer.ProduceAsync(
            _options.DlqTopic,
            new Message<Null, string>
            {
                Value = JsonSerializer.Serialize(dlq)
            },
            cancellationToken
        );
    }

    private static string? GetHeaderValue(Headers headers, string key)
    {
        var header = headers.FirstOrDefault(h => h.Key == key);
        return header == null ? null : Encoding.UTF8.GetString(header.GetValueBytes());
    }
}
