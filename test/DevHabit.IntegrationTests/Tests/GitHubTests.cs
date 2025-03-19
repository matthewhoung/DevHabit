using DevHabit.Api.DTOs.GitHub;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using DevHabit.Api.Services;
using DevHabit.IntegrationTests.Infrastructure;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace DevHabit.IntegrationTests.Tests;

public sealed class GitHubTests(DevHabitWebAppFactory factory) : IntegrationTestFixture(factory)
{
    private const string TestAccessToken = "gho_test123456789";

    private static readonly GitHubUserProfileDto User = new(
        Login: "testuser",
        Name: "Test User",
        AvatarUrl: "https://github.com/testuser.png",
        Bio: "Test bio",
        PublicRepos: 10,
        Followers: 20,
        Following: 30
    );

    private static readonly GitHubEventDto TestEvent = new(
        Id: "1234567890",
        Type: "PushEvent",
        Actor: new GitHubActorDto(
            Id: 1,
            Login: "testuser",
            DisplayLogin: "testuser",
            AvatarUrl: "https://github.com/testuser.png"
        ),
        Repository: new GitHubRepositoryDto(
            Id: 1,
            Name: "testuser/repo",
            Url: "https://api.github.com/repos/testuser/repo"
        ),
        Payload: new GitHubPayloadDto(
            Action: "test-action",
            Ref: "refs/heads/main",
            Commits:
            [
                new GitHubCommitDto(
                    Sha: "abc123",
                    Message: "Test commit",
                    Url: "https://github.com/testuser/repo/commit/abc123"
                )
            ]
        ),
        IsPublic: true,
        CreatedAt: DateTime.Parse("2025-01-01T00:00:00Z", CultureInfo.InvariantCulture)
    );

    [Fact]
    public async Task GetProfile_ShouldReturnUserProfile_WhenAccessTokenIsValid()
    {
        // Arrange
        WireMockServer
            .Given(Request.Create()
                .WithPath("/user")
                .WithHeader("Authorization", $"Bearer {TestAccessToken}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(User));

        HttpClient client = await CreateAuthenticatedClientAsync();

        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 30
        };
        await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, dto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.GitHub.GetProfile);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        GitHubUserProfileDto? profile = JsonConvert.DeserializeObject<GitHubUserProfileDto>(
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(profile);
        Assert.Equal(User.Login, profile.Login);
        Assert.Equal(User.Name, profile.Name);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        HttpClient client = CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.GitHub.GetProfile);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnNotFound_WhenNoTokenStored()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.GitHub.GetProfile);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnNotFound_WhenGitHubReturnsError()
    {
        // Arrange
        WireMockServer
            .Given(Request.Create()
                .WithPath("/user")
                .WithHeader("Authorization", $"Bearer {TestAccessToken}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.Unauthorized));

        HttpClient client = await CreateAuthenticatedClientAsync();
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 30
        };
        await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, dto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.GitHub.GetProfile);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task StoreAccessToken_ShouldSucceed_WithValidToken()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 30
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, dto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task StoreAccessToken_ShouldFail_WithInvalidToken()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = string.Empty,
            ExpiresInDays = 30
        };

        // Act
        HttpResponseMessage response = await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RevokeAccessToken_ShouldSucceed_WhenTokenExists()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();
        
        // Store a token first
        var storeDto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 30
        };
        await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, storeDto);

        // Act
        HttpResponseMessage response = await client.DeleteAsync(Routes.GitHub.RevokeAccessToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RevokeAccessToken_ShouldSucceed_WhenTokenDoesNotExist()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await client.DeleteAsync(Routes.GitHub.RevokeAccessToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnEvents_WhenAccessTokenIsValid()
    {
        // Arrange
        // Mock user profile request
        WireMockServer
            .Given(Request.Create()
                .WithPath("/user")
                .WithHeader("Authorization", $"Bearer {TestAccessToken}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(User));

        // Mock events request
        WireMockServer
            .Given(Request.Create()
                .WithPath($"/users/{User.Login}/events")
                .WithHeader("Authorization", $"Bearer {TestAccessToken}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(new[] { TestEvent }));

        HttpClient client = await CreateAuthenticatedClientAsync();
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 30
        };
        await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, dto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.GitHub.GetEvents);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<GitHubEventDto>? events = JsonConvert.DeserializeObject<List<GitHubEventDto>>(
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(events);
        Assert.Single(events);
        Assert.Equal(TestEvent.Id, events[0].Id);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnUnauthorized_WhenNoTokenStored()
    {
        // Arrange
        HttpClient client = await CreateAuthenticatedClientAsync();

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.GitHub.GetEvents);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnNotFound_WhenGitHubReturnsError()
    {
        // Arrange
        // Mock user profile request with error
        WireMockServer
            .Given(Request.Create()
                .WithPath("/user")
                .WithHeader("Authorization", $"Bearer {TestAccessToken}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.Unauthorized));

        HttpClient client = await CreateAuthenticatedClientAsync();
        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 30
        };
        await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, dto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.GitHub.GetEvents);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProfile_ShouldIncludeHateoasLinks_WhenRequested()
    {
        // Arrange
        WireMockServer
            .Given(Request.Create()
                .WithPath("/user")
                .WithHeader("Authorization", $"Bearer {TestAccessToken}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(User));

        HttpClient client = await CreateAuthenticatedClientAsync();
        client.DefaultRequestHeaders.Accept.Add(new(CustomMediaTypeNames.Application.HateoasJson));

        var dto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 30
        };
        await client.PutAsJsonAsync(Routes.GitHub.StoreAccessToken, dto);

        // Act
        HttpResponseMessage response = await client.GetAsync(Routes.GitHub.GetProfile);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        GitHubUserProfileDto? result = JsonConvert.DeserializeObject<GitHubUserProfileDto>(
            await response.Content.ReadAsStringAsync());
        Assert.NotNull(result);
        Assert.NotNull(result.Links);
        Assert.NotEmpty(result.Links);
    }
}
