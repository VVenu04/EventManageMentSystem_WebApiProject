using Application.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Presentation.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        // RequestDelegate represents the "next" middleware in the pipeline
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                // Pass the request to the next middleware (e.g., Auth -> Controller)
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                // If ANY error happens in the Controller or Service, we catch it here
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Default to 500 Internal Server Error
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = exception.Message;

            // Custom Logic: You can check specific exception types here
            // Example: If you throw "UnauthorizedAccessException", return 401
            if (exception is ArgumentException || exception is InvalidOperationException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
            }

            context.Response.StatusCode = statusCode;

            // Create your Standard API Response
            var response = ApiResponse<string>.Failure(message);

            // Convert to JSON text
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}