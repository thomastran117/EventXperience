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

        public async Task<User> SignUpAsync(string email, string password, string userType)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new ConflictException($"An account is already registered with the email: {email}");

            string hashedPassword = HashPassword(password);

            User user = new User
            {
                Email = email,
                Password = hashedPassword,
                Usertype = userType
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<UserToken> LoginAsync(string email, string password)
        {
            User user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email)
                ?? throw new UnauthorizedException("Invalid email or password");

            if (user.Password == null) throw new UnauthorizedException("Invalid email or password.");

            if (!VerifyPassword(password, user.Password)) throw new UnauthorizedException("Invalid email or password");

            Token token = _tokenService.GenerateTokens(user);

            UserToken userToken = new(token, user);

            return userToken;
        }

        private string HashPassword(string password)
        {
            using SHA256? sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }

        public async Task<UserToken> HandleTokensAsync(string refreshToken)
        {
            UserToken token = _tokenService.RotateTokens(refreshToken);
            return token;
        }

        public Task<UserToken?> GoogleAsync(string token)
        {
            throw new System.NotImplementedException();
        }

        public Task<UserToken?> MicrosoftAsync(string email, string password)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleLogoutAsync(string refreshToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
