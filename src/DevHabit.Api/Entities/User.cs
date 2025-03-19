namespace DevHabit.Api.Entities;

public sealed class User
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// We'll use this to store the IdentityId from the Identity Provider.
    /// This could be any identity provider like Azure AD, Cognito, Keycloak, Auth0, etc.
    /// </summary>
    public string IdentityId { get; set; }

    public static string NewId() => $"u_{Guid.CreateVersion7()}";
}
