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

    public async Task<User?> SignUpAsync(string email, string password, string userType)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
            throw new ConflictException("user", email);

        var hashedPassword = HashPassword(password);

        var user = new User
        {
            Email = email,
            Password = hashedPassword,
            Usertype = userType
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null) throw new NotFoundException("user", email);
        if (!VerifyPassword(password, user.Password)) throw new UnauthorizedException();

        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) throw new NotFoundException("user", "5");

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
