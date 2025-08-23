using backend.Models;

namespace backend.Interfaces;

public interface IAuthService
{
    Task<User?> SignUpAsync(string username, string password, string usertype);
    Task<User?> LoginAsync(string username, string password);
}