namespace event_indexer;

public class EventIndexerOptions
{
    public const string DefaultBootstrapServers = "kafka:9092";
    public const string DefaultTopic = "event-index-events";
    public const string DefaultGroupId = "event-indexer";
    public const string DefaultDlqTopic = "event-index-events-dlq";

    public string BootstrapServers { get; set; } = DefaultBootstrapServers;
    public string Topic { get; set; } = DefaultTopic;
    public string GroupId { get; set; } = DefaultGroupId;
    public string DlqTopic { get; set; } = DefaultDlqTopic;
}
