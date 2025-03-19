using System.Net;
using System.Net.Http.Json;
using DevHabit.Api.DTOs.Auth;
using DevHabit.IntegrationTests.Infrastructure;

namespace DevHabit.IntegrationTests.Tests;

public sealed class AuthenticationTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    [Fact]
    public async Task Register_ShouldSucceed_WithValidParameters()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Name = "register@test.com",
            Email = "register@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Register, dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_ShouldReturnAccessTokens_WithValidParameters()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Name = "register1@test.com",
            Email = "register1@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Register, dto);
        response.EnsureSuccessStatusCode();

        // Assert
        AccessTokensDto? accessTokensDto = await response.Content.ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(accessTokensDto);
        Assert.NotNull(accessTokensDto.AccessToken);
        Assert.NotNull(accessTokensDto.RefreshToken);
    }

    [Fact]
    public async Task Register_ShouldFail_WithDuplicateEmail()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Name = "duplicate@test.com",
            Email = "duplicate@test.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };
        HttpClient client = CreateClient();

        // Register first time
        await client.PostAsJsonAsync(Routes.Auth.Register, dto);

        // Act - Try to register again with same email
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Register, dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("", "test@test.com", "Test123!", "Test123!")]
    [InlineData("Test User", "", "Test123!", "Test123!")]
    [InlineData("Test User", "invalid-email", "Test123!", "Test123!")]
    [InlineData("Test User", "test@test.com", "", "Test123!")]
    [InlineData("Test User", "test@test.com", "Test123!", "")]
    [InlineData("Test User", "test@test.com", "Test123!", "DifferentPass!")]
    [InlineData("Test User", "test@test.com", "weak", "weak")]
    public async Task Register_ShouldFail_WithInvalidParameters(
        string name,
        string email,
        string password,
        string confirmPassword)
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            Name = name,
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword
        };
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Register, dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ShouldSucceed_WithValidCredentials()
    {
        // Arrange
        const string email = "login@test.com";
        const string password = "Test123!";

        // Register a user first
        var registerDto = new RegisterUserDto
        {
            Name = email,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
        HttpClient client = CreateClient();
        await client.PostAsJsonAsync(Routes.Auth.Register, registerDto);

        // Prepare login request
        var loginDto = new LoginUserDto
        {
            Email = email,
            Password = password
        };

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Login, loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AccessTokensDto? tokens = await response.Content.ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(tokens);
        Assert.NotNull(tokens.AccessToken);
        Assert.NotNull(tokens.RefreshToken);
    }

    [Fact]
    public async Task Login_ShouldFail_WithInvalidCredentials()
    {
        // Arrange
        var loginDto = new LoginUserDto
        {
            Email = "nonexistent@test.com",
            Password = "WrongPass123!"
        };
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Login, loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("", "Test123!")]
    [InlineData("invalid-email", "Test123!")]
    [InlineData("test@test.com", "")]
    public async Task Login_ShouldFail_WithInvalidParameters(string email, string password)
    {
        // Arrange
        var loginDto = new LoginUserDto
        {
            Email = email,
            Password = password
        };
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Login, loginDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    /*
    [Fact]
    public async Task Refresh_ShouldSucceed_WithValidToken()
    {
        // Arrange
        await CleanupDatabaseAsync();

        const string email = "refresh@test.com";
        const string password = "Test123!";

        // Register a user first
        var registerDto = new RegisterUserDto
        {
            Name = email,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
        HttpClient client = CreateClient();
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync(Routes.Auth.Register, registerDto);
        AccessTokensDto? initialTokens = await registerResponse.Content.ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(initialTokens);

        var refreshDto = new RefreshTokenDto
        {
            RefreshToken = initialTokens.RefreshToken
        };

        await Task.Delay(1000); // Add small delay between requests

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Refresh, refreshDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AccessTokensDto? newTokens = await response.Content.ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(newTokens);
        Assert.NotNull(newTokens.AccessToken);
        Assert.NotNull(newTokens.RefreshToken);
        Assert.NotEqual(initialTokens.AccessToken, newTokens.AccessToken);
        Assert.NotEqual(initialTokens.RefreshToken, newTokens.RefreshToken);
    }
    */

    [Fact]
    public async Task Refresh_ShouldFail_WithInvalidToken()
    {
        // Arrange
        var refreshDto = new RefreshTokenDto
        {
            RefreshToken = "invalid-refresh-token"
        };
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Refresh, refreshDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Refresh_ShouldFail_WithInvalidParameters(string refreshToken)
    {
        // Arrange
        var refreshDto = new RefreshTokenDto
        {
            RefreshToken = refreshToken
        };
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(Routes.Auth.Refresh, refreshDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /*
    [Fact]
    public async Task Refresh_ShouldIssueNewTokens_WithValidToken()
    {
        // Arrange
        await CleanupDatabaseAsync();

        const string email = "refresh2@test.com";
        const string password = "Test123!";

        // Register and get initial tokens
        var registerDto = new RegisterUserDto
        {
            Name = email,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
        HttpClient client = CreateClient();
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync(Routes.Auth.Register, registerDto);
        AccessTokensDto? initialTokens = await registerResponse.Content.ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(initialTokens);

        await Task.Delay(1000); // Add small delay between requests

        // First refresh
        var firstRefreshDto = new RefreshTokenDto
        {
            RefreshToken = initialTokens.RefreshToken
        };
        HttpResponseMessage firstRefreshResponse = await client.PostAsJsonAsync(Routes.Auth.Refresh, firstRefreshDto);
        AccessTokensDto? firstRefreshTokens = await firstRefreshResponse.Content
            .ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(firstRefreshTokens);

        await Task.Delay(1000); // Add small delay between requests

        // Second refresh with new token
        var secondRefreshDto = new RefreshTokenDto
        {
            RefreshToken = firstRefreshTokens.RefreshToken
        };

        // Act
        HttpResponseMessage secondRefreshResponse = await client
            .PostAsJsonAsync(Routes.Auth.Refresh, secondRefreshDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, secondRefreshResponse.StatusCode);
        AccessTokensDto? finalTokens = await secondRefreshResponse.Content.ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(finalTokens);
        Assert.NotEqual(initialTokens.AccessToken, finalTokens.AccessToken);
        Assert.NotEqual(initialTokens.RefreshToken, finalTokens.RefreshToken);
        Assert.NotEqual(firstRefreshTokens.AccessToken, finalTokens.AccessToken);
        Assert.NotEqual(firstRefreshTokens.RefreshToken, finalTokens.RefreshToken);
    }
    */
}
