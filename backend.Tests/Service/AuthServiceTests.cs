using System;
using System.Threading.Tasks;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Services;
using backend.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace Backend.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepo;
        private readonly Mock<IOAuthService> _oauth;
        private readonly Mock<ITokenService> _tokenService;
        private readonly Mock<IEmailService> _emailService;

        private readonly AuthService _auth;

        private readonly User _user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("password123"),
            Usertype = "User"
        };

        public AuthServiceTests()
        {
            _userRepo = new Mock<IUserRepository>();
            _oauth = new Mock<IOAuthService>();
            _tokenService = new Mock<ITokenService>();
            _emailService = new Mock<IEmailService>();

            _auth = new AuthService(_userRepo.Object, _oauth.Object, _tokenService.Object, _emailService.Object);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTokenPair_WhenCredentialsValid()
        {
            _userRepo.Setup(r => r.GetUserByEmailAsync(_user.Email))
                .ReturnsAsync(_user);

            _tokenService.Setup(t => t.GenerateAccessToken(_user))
                .Returns("access");
            _tokenService.Setup(t => t.GenerateRefreshToken(_user.Id))
                .ReturnsAsync("refresh");

            var result = await _auth.LoginAsync(_user.Email, "password123");

            result.token.AccessToken.Should().Be("access");
            result.token.RefreshToken.Should().Be("refresh");
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorized_WhenWrongPassword()
        {
            _userRepo.Setup(r => r.GetUserByEmailAsync(_user.Email))
                .ReturnsAsync(_user);

            Func<Task> act = async () => await _auth.LoginAsync(_user.Email, "wrong");

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorized_WhenUserNotFound()
        {
            _userRepo.Setup(r => r.GetUserByEmailAsync(_user.Email))
                .ReturnsAsync((User?)null);

            Func<Task> act = async () => await _auth.LoginAsync(_user.Email, "anything");

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task SignUpAsync_ShouldSendVerificationEmail_WhenNewUser()
        {
            _userRepo.Setup(r => r.EmailExistsAsync(_user.Email))
                .ReturnsAsync(false);

            _tokenService.Setup(t => t.GenerateVerificationToken(It.IsAny<User>()))
                .ReturnsAsync("verifytoken");

            _emailService.Setup(e => e.SendVerificationEmailAsync(_user.Email, "verifytoken"))
                .Returns(Task.CompletedTask);

            await _auth.SignUpAsync(_user.Email, "password123", "User");

            _emailService.Verify(e => e.SendVerificationEmailAsync(_user.Email, "verifytoken"), Times.Once);
        }

        [Fact]
        public async Task SignUpAsync_ShouldThrowConflict_WhenEmailExists()
        {
            _userRepo.Setup(r => r.EmailExistsAsync(_user.Email))
                .ReturnsAsync(true);

            Func<Task> act = async () => await _auth.SignUpAsync(_user.Email, "password", "User");

            await act.Should().ThrowAsync<ConflictException>();
        }

        [Fact]
        public async Task VerifyAsync_ShouldCreateUser_AndReturnTokenPair()
        {
            var draft = new User
            {
                Email = "draft@example.com",
                Password = "h",
                Usertype = "User"
            };

            _tokenService.Setup(t => t.VerifyVerificationToken("tok"))
                .ReturnsAsync(draft);

            _userRepo.Setup(r => r.CreateUserAsync(draft))
                .ReturnsAsync(draft);

            _tokenService.Setup(t => t.GenerateAccessToken(draft)).Returns("acc");
            _tokenService.Setup(t => t.GenerateRefreshToken(draft.Id))
                .ReturnsAsync("ref");

            var result = await _auth.VerifyAsync("tok");

            result.token.AccessToken.Should().Be("acc");
            result.token.RefreshToken.Should().Be("ref");
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldSendResetEmail_WhenEmailExists()
        {
            _userRepo.Setup(r => r.EmailExistsAsync(_user.Email))
                .ReturnsAsync(true);

            _tokenService.Setup(t => t.GenerateVerificationToken(It.IsAny<User>()))
                .ReturnsAsync("reset-token");

            _emailService.Setup(e =>
                e.SendResetPasswordEmailAsync(_user.Email, "reset-token"));

            await _auth.ForgotPasswordAsync(_user.Email);

            _emailService.Verify(e =>
                e.SendResetPasswordEmailAsync(_user.Email, "reset-token"));
        }

        [Fact]
        public async Task ForgotPasswordAsync_ShouldDoNothing_WhenEmailNotFound()
        {
            _userRepo.Setup(r => r.EmailExistsAsync(_user.Email))
                .ReturnsAsync(false);

            await _auth.ForgotPasswordAsync(_user.Email);

            _emailService.Verify(
                e => e.SendResetPasswordEmailAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldUpdatePassword_WhenValidToken()
        {
            var tokenUser = new User
            {
                Email = _user.Email,
                Password = "placeholder",
                Usertype = "User"
            };

            _tokenService.Setup(t => t.VerifyVerificationToken("tok"))
                .ReturnsAsync(tokenUser);

            _userRepo.Setup(r => r.GetUserByEmailAsync(_user.Email))
                .ReturnsAsync(_user);

            _userRepo.Setup(r => r.UpdateUserAsync(_user.Id, It.IsAny<User>()))
                .ReturnsAsync(_user);

            await _auth.ChangePasswordAsync("tok", "newpass");

            _userRepo.Verify(r => r.UpdateUserAsync(_user.Id, It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task GoogleAsync_ShouldLoginExistingUser()
        {
            var oauthUser = new OAuthUser("OID1", "google@example.com", "Name", "google");

            _oauth.Setup(o => o.VerifyGoogleTokenAsync("gtok"))
                .ReturnsAsync(oauthUser);

            _userRepo.Setup(r => r.GetUserByEmailAsync(oauthUser.Email))
                .ReturnsAsync(_user);

            _tokenService.Setup(t => t.GenerateAccessToken(_user)).Returns("acc");
            _tokenService.Setup(t => t.GenerateRefreshToken(_user.Id)).ReturnsAsync("ref");

            var result = await _auth.GoogleAsync("gtok");

            result.token.AccessToken.Should().Be("acc");
        }

        [Fact]
        public async Task GoogleAsync_ShouldCreateNewUser_WhenNotExists()
        {
            var oauthUser = new OAuthUser("G1", "new@google.com", "User", "google");

            _oauth.Setup(o => o.VerifyGoogleTokenAsync("gtok"))
                .ReturnsAsync(oauthUser);

            _userRepo.Setup(r => r.GetUserByEmailAsync(oauthUser.Email))
                .ReturnsAsync((User?)null);

            _userRepo.Setup(r => r.GetUserByGoogleIdAsync(oauthUser.Id))
                .ReturnsAsync((User?)null);

            _userRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>()))
                .ReturnsAsync(new User
                {
                    Id = 99,
                    Email = oauthUser.Email,
                    Usertype = "undefined"
                });

            _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
                .Returns("acc");
            _tokenService.Setup(t => t.GenerateRefreshToken(It.IsAny<int>()))
                .ReturnsAsync("ref");

            var result = await _auth.GoogleAsync("gtok");

            result.token.AccessToken.Should().Be("acc");
        }

        [Fact]
        public async Task HandleTokensAsync_ShouldReturnNewTokenPair()
        {
            _tokenService.Setup(t => t.ValidateRefreshToken("old")).ReturnsAsync(1);

            _userRepo.Setup(r => r.GetUserAsync(1)).ReturnsAsync(_user);

            _tokenService.Setup(t => t.GenerateAccessToken(_user)).Returns("acc");
            _tokenService.Setup(t => t.GenerateRefreshToken(_user.Id)).ReturnsAsync("ref");

            var result = await _auth.HandleTokensAsync("old");

            result.token.AccessToken.Should().Be("acc");
        }

        [Fact]
        public async Task HandleLogoutAsync_ShouldCallValidate()
        {
            _tokenService.Setup(t => t.ValidateRefreshToken("ref")).ReturnsAsync(1);

            await _auth.HandleLogoutAsync("ref");

            _tokenService.Verify(t => t.ValidateRefreshToken("ref"), Times.Once);
        }
    }
}
