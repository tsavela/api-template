using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using Serilog;
using Serilog.Context;

namespace Api.Configuration
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
            }
            else
            {
                await using var originalRequestBodyStream = _recyclableMemoryStreamManager.GetStream();
                await context.Request.Body.CopyToAsync(originalRequestBodyStream);
                originalRequestBodyStream.Seek(0, SeekOrigin.Begin);
                var requestBody = await new StreamReader(originalRequestBodyStream).ReadToEndAsync();
                originalRequestBodyStream.Seek(0, SeekOrigin.Begin);
                context.Request.Body = originalRequestBodyStream;

                await using var responseBody = new MemoryStream();
                var originalResponseBodyStream = context.Response.Body;
                context.Response.Body = responseBody;

                var timer = new Stopwatch();
                timer.Start();
                await _next(context);
                timer.Stop();

                await LogRequestResponse(_logger, context.Request, requestBody, context.Response,
                    timer.ElapsedMilliseconds);

                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalResponseBodyStream);
            }
        }

        private async Task LogRequestResponse(ILogger logger, HttpRequest request,
            string requestBody, HttpResponse response,
            long timerElapsedMilliseconds)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseString = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            var pathBase = request.PathBase.Value;
            var path = request.Path.Value;
            var queryString = request.QueryString.Value;
            var resource = pathBase + path + queryString;

            using (LogContext.PushProperty("QueryString", request.QueryString.Value))
            using (LogContext.PushProperty("RequestBody", requestBody))
            using (LogContext.PushProperty("ResponseBody", responseString))
                logger.Information("{Method} {Url} {StatusCode} {Duration}ms",
                    request.Method, resource, response.StatusCode, timerElapsedMilliseconds);
        }
    }

    public static class RequestResponseBodyLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}
