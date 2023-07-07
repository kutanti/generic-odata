using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace GenericOData.Core.Services.Extensions
{
    public static class ServiceRegistrations
    {
        public static void AddSwagger(this IServiceCollection services)
        {
            throw new NotImplementedException();
        }

        public static void AddCoreApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adding the configuration settings for local env.
        /// </summary>
        public static IConfigurationBuilder AddAppSettingsJson(this IConfigurationBuilder builder, FunctionsHostBuilderContext context)
        {
            builder.AddJsonFile(Path.Combine(context.ApplicationRootPath, "local.settings.json"), optional: true, reloadOnChange: false);
            builder.AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false);
            return builder;
        }

        public static void AddTriggerPipelineServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
        }

        

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
              .HandleTransientHttpError()
              .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.BadGateway)
              .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
              .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
