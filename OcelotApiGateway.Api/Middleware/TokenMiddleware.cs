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
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        // Eğer header'da token yoksa, cookie'den deneyin
        if (token == null && context.Request.Cookies.ContainsKey("Authorization"))
        {
            var cookieToken = context.Request.Cookies["Authorization"];
            // Cookie'den alınan token için ek bir doğrulama yapabilirsiniz
            // Örneğin, "Bearer " ifadesini kontrol etmek
            if (!string.IsNullOrEmpty(cookieToken) && cookieToken.StartsWith("Bearer "))
            {
                token = cookieToken.Substring("Bearer ".Length).Trim();

                var jsonDoc = JsonDocument.Parse(token);
                token = jsonDoc.RootElement.GetProperty("token").GetString();
            }
        }

        if (token != null)
        {
            // Token'ı HttpContext üzerinde saklayın.
            context.Items["AuthToken"] = token;
        }

        await _next(context);
    }
}
