using backend.main.configurations.resource.database;
using backend.main.configurations.security;
using backend.main.models.core;
using backend.main.repositories.contracts.users;
using backend.main.repositories.implementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.test;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetUserAsync_ReturnsSanitizedUserWithoutPassword()
    {
        await using var context = CreateContext();
        context.Users.Add(new User
        {
            Id = 11,
            Email = "user@example.com",
            Password = "hash",
            Usertype = "participant",
            Username = "user11",
            GoogleID = "google-11",
            IsDisabled = true,
            AuthVersion = 4,
        });
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var user = await repository.GetUserAsync(11);

        user.Should().NotBeNull();
        user!.Password.Should().BeNull();
        user.Email.Should().Be("user@example.com");
        user.Usertype.Should().Be(AuthRoles.Participant);
        user.GoogleID.Should().Be("google-11");
        user.IsDisabled.Should().BeTrue();
        user.AuthVersion.Should().Be(4);
    }

    [Fact]
    public async Task GetAuthByEmailAsync_ReturnsPasswordHashOnlyForAuthLookup()
    {
        await using var context = CreateContext();
        context.Users.Add(new User
        {
            Id = 12,
            Email = "login@example.com",
            Password = "password-hash",
            Usertype = "organizer",
            Username = "login12",
            IsDisabled = false,
            AuthVersion = 2,
        });
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var user = await repository.GetAuthByEmailAsync("login@example.com");

        user.Should().NotBeNull();
        user!.Password.Should().Be("password-hash");
        user.Usertype.Should().Be(AuthRoles.Organizer);
        user.AuthVersion.Should().Be(2);
    }

    [Fact]
    public async Task GetUsersAsync_Slim_ReturnsSlimProjectionOnly()
    {
        await using var context = CreateContext();
        context.Users.Add(new User
        {
            Id = 13,
            Email = "slim@example.com",
            Password = "hash",
            Usertype = "volunteer",
            IsDisabled = true,
            DisabledReason = "ops",
            DisabledAtUtc = DateTime.UtcNow,
            AuthVersion = 5,
        });
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var users = await repository.GetUsersAsync(detail: UserReadDetailLevel.Slim);

        users.Should().ContainSingle();
        var user = users[0];
        user.Email.Should().Be("slim@example.com");
        user.Username.Should().Be("slim@example.com");
        user.Usertype.Should().Be(AuthRoles.Volunteer);
        user.IsDisabled.Should().BeNull();
        user.DisabledAtUtc.Should().BeNull();
        user.DisabledReason.Should().BeNull();
        user.CreatedAt.Should().BeNull();
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersAsync_Admin_ReturnsExpandedProjectionWithoutPassword()
    {
        var disabledAt = DateTime.UtcNow.AddDays(-1);

        await using var context = CreateContext();
        context.Users.Add(new User
        {
            Id = 14,
            Email = "adminlist@example.com",
            Password = "hash",
            Usertype = "participant",
            Username = "adminlist14",
            IsDisabled = true,
            DisabledReason = "review",
            DisabledAtUtc = disabledAt,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-2),
        });
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var users = await repository.GetUsersAsync(detail: UserReadDetailLevel.Admin);

        users.Should().ContainSingle();
        var user = users[0];
        user.Username.Should().Be("adminlist14");
        user.IsDisabled.Should().BeTrue();
        user.DisabledReason.Should().Be("review");
        user.DisabledAtUtc.Should().BeCloseTo(disabledAt, TimeSpan.FromSeconds(1));
        user.CreatedAt.Should().NotBeNull();
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdsAsync_PreservesRequestedOrderAndDetailShape()
    {
        await using var context = CreateContext();
        context.Users.AddRange(
            new User
            {
                Id = 21,
                Email = "first@example.com",
                Password = "hash",
                Usertype = "participant",
                Username = "first21",
            },
            new User
            {
                Id = 22,
                Email = "second@example.com",
                Password = "hash",
                Usertype = "organizer",
                IsDisabled = true,
                DisabledReason = "paused",
            }
        );
        await context.SaveChangesAsync();

        var repository = new UserRepository(context);

        var users = await repository.GetByIdsAsync([22, 21], UserReadDetailLevel.Admin);

        users.Select(u => u.Id).Should().Equal(22, 21);
        users[0].DisabledReason.Should().Be("paused");
        users[1].DisabledReason.Should().BeNull();
    }

    private static AppDatabaseContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AppDatabaseContext(options);
    }
}
