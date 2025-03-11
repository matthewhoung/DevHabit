namespace DevHabit.Api.Entities;

public sealed class User
{
    public string Id { get; set; }
    public string IdentityId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
