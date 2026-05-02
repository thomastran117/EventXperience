namespace event_indexer;

public sealed record EventIndexerDlqRecord
{
    public required string SourceTopic { get; init; }
    public required int Partition { get; init; }
    public required long Offset { get; init; }
    public string? Key { get; init; }
    public string? EventType { get; init; }
    public required string Value { get; init; }
    public required string Error { get; init; }
    public DateTime FailedAtUtc { get; init; } = DateTime.UtcNow;
}
