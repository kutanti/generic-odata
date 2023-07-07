using GenericOData.Core.Services.Exceptions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace GenericODataAPI.Extensions
{
    public static class CustomProblemDetails
    {
        public static void AddCustomProblemDetails(this IServiceCollection services)
        {
            services.AddProblemDetails(ConfigureProblemDetails);
        }

        private static void ConfigureProblemDetails(ProblemDetailsOptions options)
        {
            options.IncludeExceptionDetails = (context, ex) =>
            {
                var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
                return environment.IsDevelopment();
            };

            options.Map<JsonException>(exception => new ProblemDetails
            {
                Status = 500,
                Title = $"{exception.GetType().Name}: {exception.Message}",
                Detail = exception.StackTrace,
                Type = exception.HelpLink
            });

            options.Map<GenericODataException>(exception => new ProblemDetails
            {
                Status = (int)exception.StatusCode,
                Title = $"{exception.GetType().Name}: {exception.Message} {exception.StatusCode.ToString()}",
                Detail = exception.StackTrace,
                Type = exception.HelpLink
            });
        }
    }
}
