using EntitySchemaGenerator.Clients;
using EntitySchemaGenerator.Contracts;
using EntitySchemaGenerator.Handlers;
using EntitySchemaGenerator.Options;
using EntitySchemaGenerator.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace EntitySchemaGenerator
{
    public static class Startup
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddLogging();
            services.AddHttpClient<MetadataRestClient>(options =>
            {
                options.BaseAddress = configuration.GetValue<Uri>("ApiMetadata:BaseUri");

            });

            services.Configure<ApiMetadataOptions>(options =>
               configuration
                   .GetSection("ApiMetadata")
                   .Bind(options)
            );
            services.Configure<StorageConfigOptions>(options =>
               configuration
                   .GetSection("Storage")
                   .Bind(options)
            );
            services.AddHostedService<EntitySchemaGenerator>();
            services.AddSingleton<AvroSchemaHandler>();
            services.AddSingleton<AzureBlobService>();
            services.AddSingleton<IEntityGeneratorService, EntityGeneratorService>();
        }
    }
}
