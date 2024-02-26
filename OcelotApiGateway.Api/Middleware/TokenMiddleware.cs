namespace OcelotApiGateway.Api.Middleware;

public class TokenMiddleware
{
    private readonly RequestDelegate _next;

    public TokenMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token != null)
        {
            // Token'ı HttpContext üzerinde saklayın.
            context.Items["AuthToken"] = token;
        }

        await _next(context);
    }
}
