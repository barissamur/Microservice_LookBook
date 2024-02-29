using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

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
        // servislerin kendi dahil ocelot dışında tüm istekleri 403 döndür. gelen istek ocelot'tan mı geliyor
        //var request = context.Request;

        //var fromOcelot = context.Request.Headers["X-Ocelot-Secret"].FirstOrDefault();

        //if (fromOcelot != "fromOcelot")

        //{
        //    context.Response.StatusCode = 403; // Erişim Reddedildi
        //    await context.Response.WriteAsync("Access denied");
        //}


        // kullanıcı id ve name headera ekle
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

