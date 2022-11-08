public static class RequestTracingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTracing(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTracingMiddleware>();
    }
}
