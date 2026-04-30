namespace backend.main.repositories.contracts.users
{
    public sealed class UserStatusRecord
    {
        public int Id { get; init; }
        public bool IsDisabled { get; init; }
        public DateTime? DisabledAtUtc { get; init; }
        public string? DisabledReason { get; init; }
        public int AuthVersion { get; init; }
    }
}
