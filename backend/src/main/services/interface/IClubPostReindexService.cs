namespace backend.main.services.interfaces
{
    public interface IClubPostReindexService
    {
        Task<int> ReindexAllAsync();
    }
}
