namespace backend.main.services.interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile image, string folder);
        Task DeleteImageAsync(string imageUrl);

    }
}
