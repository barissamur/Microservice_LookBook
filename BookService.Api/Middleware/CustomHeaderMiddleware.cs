using System.Security.Claims;

namespace OcelotApiGateway.Api.Middleware;


public class CustomHeaderMiddleware
{
    private readonly RequestDelegate _next;


    public CustomHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userId = context.Request.Headers["X-User-Id"].FirstOrDefault();
        var userName = context.Request.Headers["X-User-Name"].FirstOrDefault();

        var claims = new List<Claim>();
        if (!string.IsNullOrEmpty(userId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        if (!string.IsNullOrEmpty(userName))
        {
            claims.Add(new Claim(ClaimTypes.Name, userName));
        }

        var identity = new ClaimsIdentity(claims, "Custom");
        context.User = new ClaimsPrincipal(identity);

        await _next(context);
    }
}

