using Microsoft.Extensions.Caching.Distributed;

namespace TaskFlow.Gateway.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache _cache;

    public IdempotencyMiddleware(RequestDelegate next, IDistributedCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            await _next(context);
            return;
        }

        var key = idempotencyKey.ToString();
        var response = await _cache.GetStringAsync(key);

        if (response is not null)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(response);
            return;
        }

        var lockKey = $"lock:{key}";
        var lockAcquired = false;
        try
        {
            lockAcquired = await _cache.GetStringAsync(lockKey) is null;
            if (lockAcquired)
            {
                await _cache.SetStringAsync(lockKey, "locked", new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10) });

                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                response = await responseBody.ReadAsStringAsync();
                await _cache.SetStringAsync(key, response, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });

                responseBody.Position = 0;
                await responseBody.CopyToAsync(originalBodyStream);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsync("Request already in progress");
            }
        }
        finally
        {
            if (lockAcquired)
            {
                await _cache.RemoveAsync(lockKey);
            }
        }
    }
}

public static class IdempotencyMiddlewareExtensions
{
    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}
