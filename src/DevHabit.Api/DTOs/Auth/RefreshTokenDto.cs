namespace DevHabit.Api.DTOs.Auth;

public sealed record class RefreshTokenDto
{
    public required string RefreshToken { get; init; }
}
