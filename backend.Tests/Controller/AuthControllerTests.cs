using backend.Controllers;
using backend.Common;
using backend.DTOs;
using backend.Exceptions;
using backend.Interfaces;
using backend.Models;
using backend.Utilities;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Backend.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authService;
        private readonly AuthController _controller;

        private readonly User _user = new User
        {
            Id = 1,
            Email = "test@example.com",
            Password = "hashed",
            Usertype = "User"
        };

        private readonly UserToken _userToken;

        public AuthControllerTests()
        {
            _authService = new Mock<IAuthService>();

            _controller = new AuthController(_authService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            _userToken = new UserToken(
                new Token("access123", "refresh123"),
                _user
            );
        }

        [Fact]
        public async Task LocalAuthenticate_ShouldReturnOk_WhenValid()
        {
            var req = new LoginRequest { Email = _user.Email, Password = "pass", Captcha = "mock" };
            _authService.Setup(a => a.LoginAsync(req.Email, req.Password))
                        .ReturnsAsync(_userToken);

            var result = await _controller.LocalAuthenticate(req) as ObjectResult;

            result!.StatusCode.Should().Be(200);
            var response = result.Value as ApiResponse<AuthResponse>;
            response!.Data.Username.Should().Be(_user.Email);
        }

        [Fact]
        public async Task LocalAuthenticate_ShouldReturnError_WhenServiceFails()
        {
            var req = new LoginRequest { Email = "x", Password = "x", Captcha = "mock" };
            _authService.Setup(a => a.LoginAsync(req.Email, req.Password))
                        .ThrowsAsync(new UnauthorizedException("Invalid"));

            var result = await _controller.LocalAuthenticate(req);

            result.Should().BeOfType<ObjectResult>();
            (result as ObjectResult)!.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task LocalSignup_ShouldReturn200_OnSuccess()
        {
            var req = new SignUpRequest { Email = _user.Email, Password = "pass", Usertype = "User", Captcha = "mock" };

            var result = await _controller.LocalSignup(req) as ObjectResult;

            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task LocalSignup_ShouldReturnError_WhenConflict()
        {
            var req = new SignUpRequest { Email = _user.Email, Password = "p", Usertype = "User", Captcha = "mock" };

            _authService.Setup(a => a.SignUpAsync(req.Email, req.Password, req.Usertype))
                        .ThrowsAsync(new ConflictException("exists"));

            var result = await _controller.LocalSignup(req);
            var error = result as ObjectResult;

            error!.StatusCode.Should().Be(409);
        }

        [Fact]
        public async Task LocalVerify_ShouldReturnOk()
        {
            _authService.Setup(a => a.VerifyAsync("tok"))
                        .ReturnsAsync(_userToken);

            var result = await _controller.LocalVerify("tok") as ObjectResult;

            result!.StatusCode.Should().Be(200);
            (result.Value as ApiResponse<AuthResponse>)!.Data.Id.Should().Be(_user.Id);
        }

        [Fact]
        public async Task GoogleAuthenticate_ShouldReturnOk()
        {
            var req = new GoogleRequest { Token = "abc" };

            _authService.Setup(a => a.GoogleAsync("abc"))
                        .ReturnsAsync(_userToken);

            var result = await _controller.GoogleAuthenticate(req) as ObjectResult;

            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task MicrosoftAuthenticate_ShouldReturnOk()
        {
            var req = new MicrosoftRequest { Token = "abc" };

            _authService.Setup(a => a.MicrosoftAsync("abc"))
                        .ReturnsAsync(_userToken);

            var result = await _controller.MicrosoftAuthenticate(req) as ObjectResult;

            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Refresh_ShouldReturnOk_WhenValidToken()
        {
            _controller.HttpContext.Request.Cookies = new MockRequestCookieCollection(("refreshToken", "good"));

            _authService.Setup(a => a.HandleTokensAsync("good"))
                        .ReturnsAsync(_userToken);

            var result = await _controller.Refresh() as ObjectResult;

            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Refresh_ShouldReturnUnauthorized_WhenMissingCookie()
        {
            var result = await _controller.Refresh() as ObjectResult;

            result!.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Logout_ShouldReturnAlreadyLoggedOut_IfNoCookie()
        {
            var result = await _controller.Logout() as ObjectResult;

            result!.StatusCode.Should().Be(200);
            (result.Value as MessageResponse)!.Message.Should().Contain("already logged out");
        }

        [Fact]
        public async Task Logout_ShouldCallService_WhenCookieExists()
        {
            _controller.HttpContext.Request.Cookies = new MockRequestCookieCollection(("refreshToken", "abc"));

            var result = await _controller.Logout() as ObjectResult;

            result!.StatusCode.Should().Be(200);
            _authService.Verify(a => a.HandleLogoutAsync("abc"), Times.Once);
        }

        [Fact]
        public async Task ForgotPassword_ShouldReturnOk()
        {
            var req = new ForgotPasswordRequest { Email = _user.Email };

            var result = await _controller.ForgotPassword(req) as ObjectResult;

            result!.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task ChangePassword_ShouldReturnOk()
        {
            var req = new ChangePasswordRequest { Password = "newPass" };

            var result = await _controller.ChangePassword(req, "tok") as ObjectResult;

            result!.StatusCode.Should().Be(200);
        }
    }

    public class MockRequestCookieCollection : IRequestCookieCollection
    {
        private readonly Dictionary<string, string> _cookies;

        public MockRequestCookieCollection(params (string Key, string Value)[] cookies)
        {
            _cookies = cookies.ToDictionary(c => c.Key, c => c.Value);
        }

        public string this[string key] => _cookies.ContainsKey(key) ? _cookies[key] : null;
        public int Count => _cookies.Count;
        public ICollection<string> Keys => _cookies.Keys;
        public bool ContainsKey(string key) => _cookies.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _cookies.GetEnumerator();
        public bool TryGetValue(string key, out string value) => _cookies.TryGetValue(key, out value);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _cookies.GetEnumerator();
    }
}
