using Entities;
using Entities.DomainObjects;
using Entities.Intertfaces;
using GenericOData.Core.Services.Helper;
using GenericOData.Core.Services.Helper.Interface;
using GenericOData.Core.Services.Service;
using GenericOData.Core.Services.Service.Interface;
using GenericOData.Core.Services.Util;
using GenericODataAPI.ControllerFactory;
using GenericODataAPI.Extensions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using System;

namespace GenericODataAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddScoped<IBaseEntity, BaseEntity>();
            services.AddScoped(typeof(IDataService<>), typeof(HttpService<>));
            services.AddScoped<IEdmModelBuilder, EdmModelBuilder>();
            services.AddScoped<IODataQueryConverter, ODataQueryConverter>();
            services.AddRouting();
            services.AddHttpClient("ApiClient", options =>
            {
                options.BaseAddress = new Uri("ApiMetadata:BaseUri");
            });

            services.AddControllers().AddOData(opt =>
            {

                opt.AddRouteComponents("odata", GetEdmModel());
                opt.EnableQueryFeatures();
            });

            services.AddCustomProblemDetails();
            var mvcBuilder = services.AddMvc();
            mvcBuilder.AddMvcOptions(o => o.Conventions.Add(new GenericODataControllerNameConvention()));
            mvcBuilder.ConfigureApplicationPartManager(c =>
            {
                c.FeatureProviders.Add(new GenericODataControllerFeatureProvider());
            });

            services.AddSwaggerGen(options =>
            {

                options.CustomSchemaIds(type => type.ToString());
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Generic OData API",
                    Version = "v1",
                    Description = "OData V4 endpoints for the Datasets",
                    Contact = new OpenApiContact
                    {
                        Name = "Kunal Tanti",
                        Email = "kunal.tanti@outlook.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Proprietary: internal use only"
                    }
                });
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseProblemDetails();

            app.UseHttpsRedirection();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Generic OData API");
                c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            var dTypes = DomainObjectUtil.GetAllDomainObjectTypes();
            foreach (var _type in dTypes)
            {
                var genericBase = typeof(EntityBaseModel<>);
                var combinedType = genericBase.MakeGenericType(_type);
                builder.AddComplexType(combinedType);
            }
            return builder.GetEdmModel();
        }
    }
}