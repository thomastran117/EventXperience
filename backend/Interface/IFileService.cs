namespace backend.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile image, string folder);
        Task<bool> DeleteImageAsync(string imageUrl);

    }
}