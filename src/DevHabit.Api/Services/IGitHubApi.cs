using DevHabit.Api.DTOs.GitHub;
using Refit;

namespace DevHabit.Api.Services;

[Headers("User-Agent: DevHabit/1.0", "Accept: application/vnd.github+json")]
public interface IGitHubApi
{
    [Get("/user")]
    Task<ApiResponse<GitHubUserProfileDto>> GetUserProfile(
        [Authorize(scheme: "Bearer")] string accessToken,
        CancellationToken cancellationToken = default);

    [Get("/users/{username}/events")]
    Task<ApiResponse<List<GitHubEventDto>>> GetUserEvents(
        string username,
        [Authorize(scheme: "Bearer")] string accessToken,
        int page = 1,
        [AliasAs("per_page")] int perPage = 100,
        CancellationToken cancellationToken = default);
}
