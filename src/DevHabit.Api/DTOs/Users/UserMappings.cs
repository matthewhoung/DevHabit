using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.Entities;

namespace DevHabit.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto)
    {
        return new User
        {
            Id = User.NewId(),
            Name = dto.Name,
            Email = dto.Email,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
