using backend.main.dtos;
using backend.main.dtos.general;
using backend.main.models.other;
using backend.main.publishers.interfaces;
using backend.main.repositories.interfaces;
using backend.main.services.implementation;
using backend.main.services.interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace backend.test;

public class AuthServiceTests
{
    [Fact]
    public async Task ForgotPasswordAsync_ReturnsPlaceholderChallenge_WhenEmailDoesNotExist()
    {
        var userRepository = new Mock<IUserRepository>();
        var publisher = new Mock<IPublisher>(MockBehavior.Strict);

        userRepository.Setup(repository => repository.EmailExistsAsync("missing@example.com"))
            .ReturnsAsync(false);

        var service = CreateService(userRepository, publisher);

        var challenge = await service.ForgotPasswordAsync("missing@example.com");

        challenge.Challenge.Should().NotBeNullOrWhiteSpace();
        challenge.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow.AddMinutes(25));
        publisher.Verify(
            client => client.PublishAsync(It.IsAny<string>(), It.IsAny<EmailMessage>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ForgotPasswordAsync_ReturnsRealChallenge_WhenEmailExists()
    {
        var userRepository = new Mock<IUserRepository>();
        var tokenService = new Mock<ITokenService>();
        var publisher = new Mock<IPublisher>();
        var expectedChallenge = new VerificationOtpChallenge
        {
            Code = "123456",
            Challenge = "challenge-id",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30),
        };

        userRepository.Setup(repository => repository.EmailExistsAsync("user@example.com"))
            .ReturnsAsync(true);
        tokenService.Setup(service => service.GenerateVerificationArtifactsAsync(
                It.IsAny<backend.main.models.core.User>(),
                VerificationPurpose.ResetPassword
            ))
            .ReturnsAsync(new VerificationArtifacts
            {
                LinkToken = "link-token",
                OtpChallenge = expectedChallenge,
                Purpose = VerificationPurpose.ResetPassword,
            });
        publisher.Setup(client => client.PublishAsync("eventxperience-email", It.IsAny<EmailMessage>()))
            .Returns(Task.CompletedTask);

        var service = CreateService(userRepository, publisher, tokenService);

        var challenge = await service.ForgotPasswordAsync("user@example.com");

        challenge.Should().BeEquivalentTo(expectedChallenge);
    }

    private static AuthService CreateService(
        Mock<IUserRepository> userRepository,
        Mock<IPublisher> publisher,
        Mock<ITokenService>? tokenService = null
    )
    {
        return new AuthService(
            userRepository.Object,
            Mock.Of<IOAuthService>(),
            (tokenService ?? new Mock<ITokenService>()).Object,
            publisher.Object,
            Mock.Of<IDeviceService>(),
            new ClientRequestInfo()
        );
    }
}
