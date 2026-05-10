using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Repositories.Interfaces;
using ECommerceAPI.Services;

namespace ECommerceAPI.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();

        var configValues = new Dictionary<string, string?>
        {
            { "Jwt:Key", "SuperSecretKey_ForTesting_MinLength32Chars!" },
            { "Jwt:Issuer", "ECommerceAPI" },
            { "Jwt:Audience", "ECommerceClient" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _authService = new AuthService(_userRepoMock.Object, configuration);
    }

    [Fact]
    public async Task RegisterAsync_ValidData_ReturnsTokenAndUser()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _userRepoMock.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(1);

        var dto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password123"
        };

        var result = await _authService.RegisterAsync(dto);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Username.Should().Be("testuser");
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        _userRepoMock.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(true);

        var dto = new RegisterDto
        {
            Username = "user2",
            Email = "existing@example.com",
            Password = "password123"
        };

        var act = async () => await _authService.RegisterAsync(dto);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*registered*");
    }

    [Theory]
    [InlineData("ab", "test@test.com", "password123")]   // username too short
    [InlineData("user", "notanemail", "password123")]     // bad email
    [InlineData("user", "test@test.com", "12345")]        // password too short
    public async Task RegisterAsync_InvalidInput_ThrowsArgumentException(
        string username, string email, string password)
    {
        var dto = new RegisterDto { Username = username, Email = email, Password = password };

        var act = async () => await _authService.RegisterAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("mypassword");
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(new User
            {
                Id = 1,
                Username = "testuser",
                Email = "user@example.com",
                PasswordHash = hash
            });

        var dto = new LoginDto { Email = "user@example.com", Password = "mypassword" };

        var result = await _authService.LoginAsync(dto);

        result.Token.Should().NotBeNullOrEmpty();
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correctpassword");
        _userRepoMock.Setup(r => r.GetByEmailAsync("user@example.com"))
            .ReturnsAsync(new User
            {
                Id = 1,
                Username = "testuser",
                Email = "user@example.com",
                PasswordHash = hash
            });

        var dto = new LoginDto { Email = "user@example.com", Password = "wrongpassword" };

        var act = async () => await _authService.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User?)null);

        var dto = new LoginDto { Email = "ghost@example.com", Password = "whatever" };

        var act = async () => await _authService.LoginAsync(dto);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
