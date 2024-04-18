
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class CacheControlMiddleware
{
    private readonly RequestDelegate _next;

    public CacheControlMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
        context.Response.Headers.Add("Pragma", "no-cache");
        context.Response.Headers.Add("Expires", "0");

        await _next(context);
    }
}

