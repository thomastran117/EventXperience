using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Threading.Tasks;

using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Services;

using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Backend.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly Mock<ICacheService> _cacheMock;
        private readonly TokenService _tokenService;

        private readonly User _user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Password = "hashed_pass",
            Usertype = "User"
        };

        public TokenServiceTests()
        {
            _cacheMock = new Mock<ICacheService>();
            _tokenService = new TokenService(_cacheMock.Object);
        }

        [Fact]
        public void GenerateAccessToken_ShouldReturn_AValidJwtToken()
        {
            string token = _tokenService.GenerateAccessToken(_user);

            token.Should().NotBeNullOrWhiteSpace();

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            jwt.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "1");
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == _user.Email);
            jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == _user.Usertype);
        }
        
        [Fact]
        public async Task GenerateRefreshToken_ShouldReturn_NewUniqueToken()
        {
            _cacheMock.Setup(c => c.GetValueAsync(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            _cacheMock.Setup(c => c.SetValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(true);

            string token = await _tokenService.GenerateRefreshToken(_user.Id);

            token.Should().NotBeNullOrWhiteSpace();

            _cacheMock.Verify(c => c.SetValueAsync($"refresh:{token}", "1", It.IsAny<TimeSpan?>()));
        }

        [Fact]
        public async Task GenerateRefreshToken_ShouldRetry_OnCollision()
        {
            _cacheMock.SetupSequence(c => c.GetValueAsync(It.IsAny<string>()))
                .ReturnsAsync("collision")
                .ReturnsAsync((string?)null);

            _cacheMock.Setup(c => c.SetValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(true);

            string token = await _tokenService.GenerateRefreshToken(_user.Id);

            token.Should().NotBeNullOrWhiteSpace();
            _cacheMock.Verify(c => c.GetValueAsync(It.IsAny<string>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task GenerateRefreshToken_ShouldThrow_WhenCacheFails()
        {
            _cacheMock.Setup(c => c.GetValueAsync(It.IsAny<string>()))
                .ReturnsAsync((string?)null);

            _cacheMock.Setup(c => c.SetValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await _tokenService.GenerateRefreshToken(_user.Id);

            await act.Should().ThrowAsync<NotAvaliableException>();
        }

        [Fact]
        public async Task ValidateRefreshToken_ShouldReturnUserId_AndDeleteKey()
        {
            _cacheMock.Setup(c => c.GetValueAsync("refresh:abc")).ReturnsAsync("1");
            _cacheMock.Setup(c => c.DeleteKeyAsync("refresh:abc")).ReturnsAsync(true);

            int result = await _tokenService.ValidateRefreshToken("abc");

            result.Should().Be(1);
        }

        [Fact]
        public async Task ValidateRefreshToken_ShouldThrow_WhenTokenMissing()
        {
            _cacheMock.Setup(c => c.GetValueAsync("refresh:abc"))
                .ReturnsAsync((string?)null);

            Func<Task> act = async () => await _tokenService.ValidateRefreshToken("abc");

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task ValidateRefreshToken_ShouldThrow_WhenDeleteFails()
        {
            _cacheMock.Setup(c => c.GetValueAsync("refresh:abc")).ReturnsAsync("1");
            _cacheMock.Setup(c => c.DeleteKeyAsync("refresh:abc")).ReturnsAsync(false);

            Func<Task> act = async () => await _tokenService.ValidateRefreshToken("abc");

            await act.Should().ThrowAsync<NotAvaliableException>();
        }

        [Fact]
        public async Task GenerateVerificationToken_ShouldStoreSerializedUser()
        {
            _cacheMock.Setup(c => c.SetValueAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
                .ReturnsAsync(true);

            string token = await _tokenService.GenerateVerificationToken(_user);

            token.Should().NotBeNullOrWhiteSpace();

            _cacheMock.Verify(c => c.SetValueAsync(
                It.Is<string>(k => k.StartsWith("verify:")),
                It.Is<string>(v => v.Contains("test@example.com")),
                It.IsAny<TimeSpan?>()));
        }

        [Fact]
        public async Task VerifyVerificationToken_ShouldReturnUser_AndDeleteKey()
        {
            var payload = JsonConvert.SerializeObject(new User
            {
                Email = _user.Email,
                Password = _user.Password,
                Usertype = _user.Usertype
            });

            _cacheMock.Setup(c => c.GetValueAsync("verify:xyz")).ReturnsAsync(payload);
            _cacheMock.Setup(c => c.DeleteKeyAsync("verify:xyz")).ReturnsAsync(true);

            User processed = await _tokenService.VerifyVerificationToken("xyz");

            processed.Email.Should().Be(_user.Email);
            processed.Password.Should().Be(_user.Password);
            processed.Usertype.Should().Be(_user.Usertype);
        }

        [Fact]
        public async Task VerifyVerificationToken_ShouldThrow_WhenMissing()
        {
            _cacheMock.Setup(c => c.GetValueAsync("verify:xyz"))
                .ReturnsAsync((string?)null);

            Func<Task> act = async () => await _tokenService.VerifyVerificationToken("xyz");

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task VerifyVerificationToken_ShouldThrow_WhenDeleteFails()
        {
            var payload = JsonConvert.SerializeObject(new User
            {
                Email = _user.Email,
                Password = _user.Password,
                Usertype = _user.Usertype
            });

            _cacheMock.Setup(c => c.GetValueAsync("verify:xyz")).ReturnsAsync(payload);
            _cacheMock.Setup(c => c.DeleteKeyAsync("verify:xyz")).ReturnsAsync(false);

            Func<Task> act = async () => await _tokenService.VerifyVerificationToken("xyz");

            await act.Should().ThrowAsync<NotAvaliableException>();
        }
    }
}
