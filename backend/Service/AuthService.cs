using System.Text;

using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

using backend.Common;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Resources;
using backend.Repositories;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private const string DummyHash = "$2a$11$9FJqO6j/4jP3E2fOQdWgMuKZXWWvPZ09f8Pj0L9VqB6TfqZ4fE5SO";

        public AuthService(UserRepository userRepository, ITokenService tokenService, IEmailService emailService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<UserToken> LoginAsync(string email, string password)
        {
            User? user = await _userRepository.GetUserByEmailAsync(email);

            var hashToCheck = user?.Password ?? DummyHash;

            bool isValidPassword = VerifyPassword(password, hashToCheck);

            if (user == null || user.Password == null || !isValidPassword)
                throw new UnauthorizedException("Invalid email or password");

            Token token = await _tokenService.GenerateTokens(user);

            UserToken userToken = new(token, user);

            return userToken;
        }

        public async Task<bool> SignUpAsync(string email, string password, string userType)
        {
            if (await _userRepository.EmailExistsAsync(email))
                throw new ConflictException($"An account is already registered with the email: {email}");

            string hashedPassword = HashPassword(password);

            User user = new User
            {
                Email = email,
                Password = hashedPassword,
                Usertype = userType
            };

            var token = await _tokenService.GenerateVerificationToken(user);
            await _emailService.SendVerificationEmailAsync(email, token);

            return true;
        }

        public async Task<UserToken> VerifyAsync(string token)
        {
            var user = await _tokenService.VerifyVerificationToken(token);

            await _userRepository.AddAsync(user);

            Token authToken = await _tokenService.GenerateTokens(user);

            UserToken userToken = new(authToken, user);

            return userToken;
        }

        public async Task<UserToken> HandleTokensAsync(string refreshToken)
        {
            UserToken token = await _tokenService.RotateTokens(refreshToken);
            return token;
        }

        public Task<UserToken> GoogleAsync(string token)
        {
            throw new System.NotImplementedException();
        }

        public Task<UserToken> MicrosoftAsync(string email, string password)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleLogoutAsync(string refreshToken)
        {
            throw new System.NotImplementedException();
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
    }
}
