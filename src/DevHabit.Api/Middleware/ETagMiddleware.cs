using DevHabit.Api.Services;

namespace DevHabit.Api.Middleware;

public sealed class ETagMiddleware
{
    public async Task InvokeAsync(HttpContext context, InMemoryETagStore eTagStore)
    {
        string resourceUri = context.Request.Path.Value;
        string? ifNoneMatch = context.Request.Headers.IfNoneMatch.FirstOrDefault()?.Replace("\"", "");
    }
}
