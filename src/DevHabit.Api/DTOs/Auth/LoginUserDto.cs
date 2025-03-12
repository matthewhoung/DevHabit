namespace DevHabit.Api.DTOs.Auth;

public sealed record class LoginUserDto
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
