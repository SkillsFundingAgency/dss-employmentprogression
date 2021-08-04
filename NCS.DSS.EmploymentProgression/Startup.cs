using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.Common.Standard.ServiceBusCleint;
using DFC.Common.Standard.ServiceBusClient.Interfaces;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Validators;
using System;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.EmploymentProgression
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEmploymentProgressionPostTriggerService, EmploymentProgressionPostTriggerService>();
            services.AddTransient<IEmploymentProgressionPatchTriggerService, EmploymentProgressionPatchTriggerService>();
            services.AddTransient<IEmploymentProgressionGetTriggerService, EmploymentProgressionGetTriggerService>();
            services.AddTransient<IEmploymentProgressionGetByIdTriggerService, EmploymentProgressionGetByIdTriggerService>();
            services.AddTransient<IEmploymentProgressionPatchService, EmploymentProgressionPatchService>();

            services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
            services.AddTransient<IServiceBusClient, ServiceBusClient>();
            services.AddTransient<IValidate, Validate>();
            services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            services.AddScoped<IGeoCodingService, GeoCodingService>();
            services.AddScoped<IAzureMapService, AzureMapService>();

            services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            services.AddSingleton<IJsonHelper, JsonHelper>();
            services.AddSingleton<IResourceHelper, ResourceHelper>();
            services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            services.AddSingleton<ILoggerHelper, LoggerHelper>();
            services.AddTransient<IGuidHelper, GuidHelper>();

            var settings = GetConfigurationSettings();
            services.AddSingleton(settings);
            services.AddSingleton<CosmosDocumentClient.ICosmosDocumentClient, CosmosDocumentClient.CosmosDocumentClient>(x => new CosmosDocumentClient.CosmosDocumentClient(settings.CosmosDBConnectionString));
        }

        private ConfigurationSettings GetConfigurationSettings()
        {
            var settings = new ConfigurationSettings
            {
                CosmosDBConnectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString"),
                KeyName = Environment.GetEnvironmentVariable("KeyName"),
                AccessKey = Environment.GetEnvironmentVariable("AccessKey"),
                BaseAddress = Environment.GetEnvironmentVariable("BaseAddress"),
                QueueName = Environment.GetEnvironmentVariable("QueueName"),
                ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString"),

                AzureMapURL = Environment.GetEnvironmentVariable("AzureMapURL"),
                AzureMapApiVersion = Environment.GetEnvironmentVariable("AzureMapApiVersion"),
                AzureMapSubscriptionKey = Environment.GetEnvironmentVariable("AzureMapSubscriptionKey"),
                AzureCountrySet = Environment.GetEnvironmentVariable("AzureCountrySet"),
            };
            return settings;
        }
    }
}
