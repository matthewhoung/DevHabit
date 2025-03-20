using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.DTOs.GitHub;
using DevHabit.FunctionalTests.Infrastructure;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Net;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.RequestBuilders;

namespace DevHabit.FunctionalTests.Tests;
public sealed class GitHubIntegrationFlowTests(DevHabitWebAppFactory factory) : FunctionalTestFixture(factory)
{
    private const string TestAccessToken = "gho_test123456789";

    private static readonly GitHubUserProfileDto TestUser = new(
        Login: "testuser",
        Name: "Test User",
        AvatarUrl: "https://github.com/testuser.png",
        Bio: "Test bio",
        PublicRepos: 10,
        Followers: 20,
        Following: 30
    );

    [Fact]
    public async Task CompleteGitHubIntegrationFlow_ShouldSucceed()
    {
        // Arrange
        await CleanupDatabaseAsync();
        const string email = "githubflow@test.com";
        const string password = "Test123!";

        // Step 1: Register a new user
        HttpClient client = CreateClient();
        var registerDto = new RegisterUserDto
        {
            Name = email,
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync(Routes.Auth.Register, registerDto);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Step 2: Login to get the token
        var loginDto = new LoginUserDto
        {
            Email = email,
            Password = password
        };
        HttpResponseMessage loginResponse = await client.PostAsJsonAsync(Routes.Auth.Login, loginDto);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        AccessTokensDto? tokens = await loginResponse.Content.ReadFromJsonAsync<AccessTokensDto>();
        Assert.NotNull(tokens);
        client.DefaultRequestHeaders.Authorization = new("Bearer", tokens.AccessToken);

        // Step 3: Mock GitHub API responses
        WireMockServer
            .Given(Request.Create()
                .WithPath("/user")
                .WithHeader("Authorization", $"Bearer {TestAccessToken}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", MediaTypeNames.Application.Json)
                .WithBodyAsJson(TestUser));

        // Step 4: Store GitHub access token
        var storeTokenDto = new StoreGitHubAccessTokenDto
        {
            AccessToken = TestAccessToken,
            ExpiresInDays = 30
        };
        HttpResponseMessage storeTokenResponse = await client.PutAsJsonAsync(
            Routes.GitHub.StoreAccessToken, storeTokenDto);
        Assert.Equal(HttpStatusCode.NoContent, storeTokenResponse.StatusCode);

        // Step 5: Get GitHub profile
        HttpResponseMessage getProfileResponse = await client.GetAsync(Routes.GitHub.GetProfile);
        Assert.Equal(HttpStatusCode.OK, getProfileResponse.StatusCode);
        GitHubUserProfileDto? profile = await getProfileResponse.Content.ReadFromJsonAsync<GitHubUserProfileDto>();
        Assert.NotNull(profile);
        Assert.Equal(TestUser.Login, profile.Login);
        Assert.Equal(TestUser.Name, profile.Name);
        Assert.Equal(TestUser.Bio, profile.Bio);

        // Step 6: Revoke GitHub access token
        HttpResponseMessage revokeTokenResponse = await client.DeleteAsync(Routes.GitHub.RevokeAccessToken);
        Assert.Equal(HttpStatusCode.NoContent, revokeTokenResponse.StatusCode);

        // Step 7: Verify profile access is revoked
        HttpResponseMessage getProfileAfterRevokeResponse = await client.GetAsync(Routes.GitHub.GetProfile);
        Assert.Equal(HttpStatusCode.NotFound, getProfileAfterRevokeResponse.StatusCode);
    }
}
