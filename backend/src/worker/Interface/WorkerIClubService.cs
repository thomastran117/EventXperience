namespace worker.Interfaces
{
    public interface IClubService
    {
        Task RevalidateClubCacheAsync(int batchSize = 100);
        Task RevalidateClubListsAsync(
            int[] pages,
            int pageSize = 20);
        Task RevalidateAllAsync();
    }
}
