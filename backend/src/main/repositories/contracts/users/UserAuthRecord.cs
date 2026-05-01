namespace backend.main.repositories.contracts.users
{
    public sealed class UserAuthRecord
    {
        public int Id { get; init; }
        public string Email { get; init; } = null!;
        public string? Password { get; init; }
        public string Usertype { get; init; } = null!;
        public bool IsDisabled { get; init; }
        public int AuthVersion { get; init; }
    }
}
