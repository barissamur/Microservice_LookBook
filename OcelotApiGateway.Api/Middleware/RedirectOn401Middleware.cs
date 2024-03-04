namespace LookBook.Web.Middleware;

public class RedirectOn401Middleware
{
    private readonly RequestDelegate _next;

    public RedirectOn401Middleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == 401 && context.Request.Path.StartsWithSegments("/Basket"))
        {
            context.Response.Redirect("/Home/Login");
        }
    }
}
