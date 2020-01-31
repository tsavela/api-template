using System;
using System.Net;
using System.Threading.Tasks;
using Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace Api.Configuration
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.None
        };

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
        {
            _logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Caught exception in {nameof(ExceptionHandlingMiddleware)}");
                await ReturnErrorDetailsAsync(httpContext, ex);
            }
        }

        private Task ReturnErrorDetailsAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)GetStatusCode(exception);

            return context.Response.WriteAsync(JsonConvert.SerializeObject(new ProblemDetails
            {
                Title = GetErrorTitle(exception),
                Detail = GetErrorDetail(exception),
                Status = context.Response.StatusCode,
                Instance = context.Request.Path
            }, JsonSerializerSettings));
        }

        private static HttpStatusCode GetStatusCode(Exception exception) => exception switch
        {
            DomainException _ => HttpStatusCode.BadRequest,
            ValidationException _ => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        private string GetErrorTitle(Exception exception) => exception switch
        {
            DomainException _ => "Business rule violation",
            ValidationException _ => "Data validation error",
            _ => "Unhandled application error"
        };

        private static string GetErrorDetail(Exception exception) => exception switch
        {
            DomainException c => c.Message,
            ValidationException v => v.Message,
            _ => "Internal server error"
        };
    }
}
