using System.Text;

using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

using backend.Common;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Resources;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDatabaseContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(AppDatabaseContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<User?> SignUpAsync(string email, string password, string userType)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new ConflictException($"An account is already registered with the email: {email}");

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

        public async Task<UserToken?> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new UnauthorizedException("Invalid email or password");

            if (!VerifyPassword(password, user.Password)) throw new UnauthorizedException("Invalid email or password");

            var token = _tokenService.GenerateTokens(user)
                ?? throw new InternalServerException("Internal server error: Unable to generate tokens");

            var userToken = new UserToken(token, user);

            return userToken;
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

        public async Task<UserToken?> HandleTokensAsync(string refreshToken)
        {
            var token = _tokenService.RotateTokens(refreshToken)
                ?? throw new InternalServerException("Internal server error: Unable to generate tokens");

            return token;
        }
    }
}