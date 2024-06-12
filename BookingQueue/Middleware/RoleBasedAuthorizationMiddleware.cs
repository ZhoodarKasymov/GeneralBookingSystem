using BookingQueue.Common.Constants;

namespace BookingQueue.Middleware;

public class RoleBasedAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public RoleBasedAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userRole = context.User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;

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