using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.CosmosDocumentClient;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.ServiceBus;
using NCS.DSS.EmploymentProgression.Validators;
using System;
using NCS.DSS.EmployeeProgression.GeoCoding;
using DFC.GeoCoding.Standard.AzureMaps.Service;


// todo make this file into a nuget package / or add to an exiting nuget package under its own namespace
namespace NCS.DSS.EmploymentProgression
{
    public static class TriggerExtensions
    {
        public static IServiceCollection AddTriggerSupport(this IServiceCollection services)
        {
            services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();

            services.AddTransient<IServiceBusClient, ServiceBusClient>();
            services.AddTransient<IValidate, Validate>();

            services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            services.AddScoped<IGeoCodingService, GeoCodingService>();
            services.AddScoped<IAzureMapService, AzureMapService>();
            return services;
        }

        public static IServiceCollection AddTriggerHelpers(this IServiceCollection services)
        {
            services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            services.AddSingleton<IJsonHelper, JsonHelper>();
            services.AddSingleton<IResourceHelper, ResourceHelper>();
            services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            services.AddSingleton<ILoggerHelper, LoggerHelper>();

            return services;
        }

        public static IServiceCollection AddTriggerSettings(this IServiceCollection services)
        {
            var settings = GetConfigurationSettings();
            services.AddSingleton(settings);
            services.AddSingleton<ICosmosDocumentClient, CosmosDocumentClient.CosmosDocumentClient>(x => new CosmosDocumentClient.CosmosDocumentClient(settings.CosmosDBConnectionString));


            return services;
        }

        private static ConfigurationSettings GetConfigurationSettings()
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
