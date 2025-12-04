using backend.Models;

namespace backend.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> UpdateUserAsync(int id, User updatedUser);
        Task<User?> UpdateAvatarAsync(int id, IFormFile image);
        Task<bool> DeleteUserAsync(int id);
    }
}
