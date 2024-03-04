using System.Text.Json;

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
        string token = "";
        token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        // Eğer header'da token yoksa, cookie'den deneyin
        if (token == null && context.Request.Cookies.ContainsKey("Authorization"))
            token = context.Request.Cookies["Authorization"].Split(" ").Last();

        await _next(context);
    }


}
