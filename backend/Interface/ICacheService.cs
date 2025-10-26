namespace backend.Interfaces
{
    public interface ICacheService
    {
        Task<bool> SetValueAsync(string key, string value, TimeSpan? expiry = null);
        Task<string?> GetValueAsync(string key);
        Task<bool> DeleteKeyAsync(string key);
        Task<bool> KeyExistsAsync(string key);
    }
}
