namespace DevHabit.IntegrationTests.Infrastructure;

public static class Routes
{
    public static class Auth
    {
        public const string Register = "auth/register";
        public const string Login = "auth/login";
        public const string Refresh = "auth/refresh";
    }

    public static class Habits
    {
        public const string GetAll = "habits";
        public const string Create = "habits";
        public static string GetById(string id) => $"habits/{id}";
        public static string Update(string id) => $"habits/{id}";
        public static string Patch(string id) => $"habits/{id}";
        public static string Delete(string id) => $"habits/{id}";
    }

    public static class GitHub
    {
        public const string StoreAccessToken = "github/personal-access-token";
        public const string RevokeAccessToken = "github/personal-access-token";
        public const string GetProfile = "github/profile";
        public const string GetEvents = "github/events";
    }

    public static class Tags
    {
        public const string GetAll = "tags";
        public const string Create = "tags";
        public static string GetById(string id) => $"tags/{id}";
        public static string Update(string id) => $"tags/{id}";
        public static string Delete(string id) => $"tags/{id}";
    }

    public static class Entries
    {
        public const string GetAll = "entries";
        public const string Create = "entries";
        public static string GetById(string id) => $"entries/{id}";
        public static string Update(string id) => $"entries/{id}";
        public static string Delete(string id) => $"entries/{id}";
        public const string CreateBatch = "entries/batch";
        public const string Stats = "entries/stats";
    }

    public static class Users
    {
        public static string GetById(string id) => $"users/{id}";
        public const string GetCurrentUser = "users/me";
    }

    public static class EntryImports
    {
        public const string GetAll = "entries/imports";
        public const string Create = "entries/imports";
        public static string GetById(string id) => $"entries/imports/{id}";
    }

    public static class HabitTags
    {
        public static string UpsertTags(string habitId) => $"habits/{habitId}/tags";
        public static string DeleteTag(string habitId, string tagId) => $"habits/{habitId}/tags/{tagId}";
    }
}
