using System.Security.Cryptography;
using System.Text;
using backend.Models;
using backend.Interfaces;
using backend.Resources;
using Microsoft.EntityFrameworkCore;
using backend.Exceptions;

namespace backend.Services;

public class AuthService : IAuthService
{
    private readonly AppDatabaseContext _context;

    public AuthService(AppDatabaseContext context)
    {
        _context = context;
    }

    public async Task<User?> SignUpAsync(string username, string password, string userType)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username))
            throw new ConflictException("user", username);

        var hashedPassword = HashPassword(password);

        var user = new User
        {
            Username = username,
            Password = hashedPassword,
            Usertype = userType
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> LoginAsync(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null) throw new NotFoundException("user", username);
        if (!VerifyPassword(password, user.Password)) throw new UnauthorizedException();

        return user;
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hashedPassword;
    }
}
