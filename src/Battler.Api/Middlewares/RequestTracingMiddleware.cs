public sealed class RequestTracingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTracingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var requestId = context.TraceIdentifier;
        context.Response.Headers.Add(CustomHeaders.RequestId, requestId);

        await _next(context);
    }
}
