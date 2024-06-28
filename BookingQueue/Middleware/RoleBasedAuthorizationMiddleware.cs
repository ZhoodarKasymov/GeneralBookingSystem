using BookingQueue.Common.Constants;

namespace BookingQueue.Middleware;

public class RoleBasedAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public RoleBasedAuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var apiPrivateKey = _configuration.GetValue<string>("ApiPrivateKey");
        var userRole = context.User.Claims.FirstOrDefault(c => c.Type == "Role")?.Value;
        var hasSamePrivateKey = context.Request.Headers.Any(h => h.Key == apiPrivateKey);

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