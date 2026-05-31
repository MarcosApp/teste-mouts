using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware
{
    public class ValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationExceptionMiddleware> _logger;

        public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        private static readonly HashSet<string> _infrastructureMessages = new(StringComparer.OrdinalIgnoreCase)
        {
            "transient failure", "connection", "timeout", "database"
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                await HandleValidationExceptionAsync(context, ex);
            }
            catch (KeyNotFoundException ex)
            {
                await HandleErrorAsync(context, StatusCodes.Status404NotFound, "ResourceNotFound", ex.Message);
            }
            catch (InvalidOperationException ex) when (!IsInfrastructureError(ex))
            {
                await HandleErrorAsync(context, StatusCodes.Status400BadRequest, "BusinessError", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleErrorAsync(context, StatusCodes.Status500InternalServerError, "InternalError",
                    "An unexpected error occurred. Please try again later.");
            }
        }

        private static bool IsInfrastructureError(Exception ex)
        {
            var msg = ex.Message + (ex.InnerException?.Message ?? string.Empty);
            return _infrastructureMessages.Any(keyword => msg.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        private static Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = new ApiResponse
            {
                Success = false,
                Message = "Validation Failed",
                Errors = exception.Errors.Select(error => (ValidationErrorDetail)error)
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        private static Task HandleErrorAsync(HttpContext context, int statusCode, string type, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var body = JsonSerializer.Serialize(
                new { type, error = message, detail = message },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return context.Response.WriteAsync(body);
        }
    }
}
