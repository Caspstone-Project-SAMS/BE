using Azure.Core;
using DocumentFormat.OpenXml.InkML;

namespace Base.API.Middleware
{
    public class WebsocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public WebsocketMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.WebSockets.IsWebSocketRequest)
            {
                var path = httpContext.Request.Path;
                if (path.StartsWithSegments("/ws/client"))
                {
                    if (httpContext.Response.StatusCode == StatusCodes.Status200OK)
                    {
                        var requestHeaders = httpContext.Request.Headers["Sec-WebSocket-Protocol"].ToString().Split(",");
                        httpContext.Response.Headers.Add("Sec-WebSocket-Protocol", requestHeaders.First().Trim());
                        //httpContext.Response.StatusCode = StatusCodes.Status200OK;
                    }
                }
            }

            await _next(httpContext);
        }
    }
}
