using Microsoft.AspNetCore.Http;

public sealed class RequestIdMiddlwareTests
{
    [Fact]
    public async Task ShouldAddRequestIdToResponseHeader()
    {
        // arrange
        var requestId = "requestId";
        var context = new DefaultHttpContext();
        context.TraceIdentifier = requestId;
        var next = new RequestTracingMiddleware(next => Task.CompletedTask);

        // act
        await next.Invoke(context);

        // assert
        context.Response.Headers
            .Should()
            .ContainKey(CustomHeaders.RequestId)
            .And.ContainValue(requestId);
    }
}
