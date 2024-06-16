using BookingQueue.Common.Constants;

namespace BookingQueue.Middleware;

public class RoleBasedAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private const string PrivateKey = "8402b7760a784d6f99c0536eefc9c2af";

    public RoleBasedAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userRole = context.User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        var hasSamePrivateKey = context.Request.Headers.Any(h => h.Key == PrivateKey);

        if (context.Request.Path.StartsWithSegments("/api") && !hasSamePrivateKey)
        {
            context.Response.StatusCode = 403;
            return;
        }
        if (context.Request.Path.StartsWithSegments("/admin/login") || context.Request.Path.StartsWithSegments("/admin/logout"))
        {
            await _next(context);
        }
        else if (context.Request.Path.StartsWithSegments("/admin") && userRole != RoleConstants.SuperAdmin)
        {
            context.Response.StatusCode = 403;
            return;
        }

        await _next(context);
    }
}