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
        Stream originalBody = context.Response.Body;

        try
        {
            using (var memStream = new MemoryStream())
            {
                context.Response.Body = memStream;

                await _next(context);

                memStream.Position = 0;
                string responseBody = new StreamReader(memStream).ReadToEnd();
                memStream.Position = 0;
                await memStream.CopyToAsync(originalBody);

                if (context.Response.StatusCode == 401 && context.Request.Path.StartsWithSegments("/Basket"))
                {
                    context.Response.Clear();
                    context.Response.StatusCode = 302; // Or use StatusCodes.Status302Found
                    context.Response.Headers.Location = "/Home/Login";
                }
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

}
