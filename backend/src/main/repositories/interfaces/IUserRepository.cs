using backend.main.repositories.contracts.users;
using backend.main.models.core;

namespace backend.main.repositories.interfaces
{
    public interface IUserRepository
    {
        Task<User> CreateUserAsync(User user);
        Task<User?> UpdateUserAsync(int id, User updated);
        Task<User?> UpdatePartialAsync(User user);
        Task<UserOAuthRecord?> UpdateProviderIdsAsync(int id, string? googleId, string? microsoftId);
        Task<UserStatusRecord?> UpdateUserStatusAsync(int id, bool isDisabled, string? disabledReason);
        Task<bool> IncrementAuthVersionAsync(int id);
        Task<bool> DeleteUserAsync(int id);
        /// <summary>
        /// Returns a sanitized User aggregate for non-auth workflows. Password is always null.
        /// </summary>
        Task<User?> GetUserAsync(int id);
        Task<UserAuthRecord?> GetAuthByEmailAsync(string email);
        Task<UserOAuthRecord?> GetOAuthByEmailAsync(string email);
        Task<UserOAuthRecord?> GetOAuthByMicrosoftIdAsync(string microsoftId);
        Task<UserOAuthRecord?> GetOAuthByGoogleIdAsync(string googleId);
        Task<UserProfileRecord?> GetProfileByUsernameAsync(string username);
        Task<IReadOnlyList<UserListRecord>> GetUsersAsync(
            string? role = null,
            UserReadDetailLevel detail = UserReadDetailLevel.Slim
        );
        Task<bool> EmailExistsAsync(string email);
        Task<IReadOnlyList<UserListRecord>> GetByIdsAsync(
            IEnumerable<int> ids,
            UserReadDetailLevel detail = UserReadDetailLevel.Slim
        );
    }
}
