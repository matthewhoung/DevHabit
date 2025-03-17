using System.Net.Http.Headers;
using DevHabit.Api.DTOs.GitHub;
using Newtonsoft.Json;
using Refit;

namespace DevHabit.Api.Services;

public sealed class RefitGitHubService(
    IGitHubApi gitHubApi, 
    ILogger<GitHubService> logger)
{
    public async Task<GitHubUserProfileDto?> GetUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(accessToken);

        ApiResponse<GitHubUserProfileDto> response = await gitHubApi.GetUserProfile(accessToken, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed to get GitHub user profile. Status code: {StatusCode}", response.StatusCode);

            return null;
        }

        return response.Content;
    }

    public async Task<IReadOnlyList<GitHubEventDto>?> GetUserEventsAsync(
        string username,
        string accessToken,
        int page = 1,
        int perPage = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(accessToken);
        ArgumentException.ThrowIfNullOrEmpty(username);

        ApiResponse<List<GitHubEventDto>> response = await gitHubApi.GetUserEvents(
            username,
            accessToken,
            page,
            perPage,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Failed to get GitHub user events. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        return response.Content;
    }

}
