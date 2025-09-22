using backend.Models;

namespace backend.Interfaces;

public interface IAuthService
{
    Task<User?> SignUpAsync(string email, string password, string usertype);
    Task<User?> LoginAsync(string email, string password);
}