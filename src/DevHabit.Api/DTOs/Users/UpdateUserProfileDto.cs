namespace DevHabit.Api.DTOs.Users;

/// <summary>
/// Data transfer object for updating a user's profile information
/// </summary>
/// <param name="Name">The user's display name</param>
public sealed record UpdateUserProfileDto(string Name);
