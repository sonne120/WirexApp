using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace WirexApp.API
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = string.Empty;

            switch (exception)
            {
                case ValidationException validationException:
                    code = HttpStatusCode.BadRequest;
                    result = JsonConvert.SerializeObject(new
                    {
                        error = "Validation failed",
                        errors = validationException.Errors
                    });
                    break;

                case ArgumentException argumentException:
                    code = HttpStatusCode.BadRequest;
                    result = JsonConvert.SerializeObject(new
                    {
                        error = "Invalid argument",
                        message = argumentException.Message
                    });
                    break;

                case InvalidOperationException invalidOperationException:
                    code = HttpStatusCode.BadRequest;
                    result = JsonConvert.SerializeObject(new
                    {
                        error = "Invalid operation",
                        message = invalidOperationException.Message
                    });
                    break;

                case UnauthorizedAccessException:
                    code = HttpStatusCode.Unauthorized;
                    result = JsonConvert.SerializeObject(new
                    {
                        error = "Unauthorized access"
                    });
                    break;

                default:
                    code = HttpStatusCode.InternalServerError;
                    result = JsonConvert.SerializeObject(new
                    {
                        error = "An internal server error occurred",
                        message = exception.Message
                    });
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
