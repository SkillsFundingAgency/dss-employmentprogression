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


// todo make this file into a nuget package / or add to an exiting nuget package under its own namespace
namespace NCS.DSS.EmploymentProgression
{
    public static class TriggerExtensions
    {
        public static IServiceCollection AddTriggerSupport(this IServiceCollection services)
        {
            services.AddTransient<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            services.AddTransient<IServiceBusClient, ServiceBusClient>();
            services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();
            services.AddTransient<IValidate, Validate>();

            return services;
        }

        public static IServiceCollection AddTriggerHelpers(this IServiceCollection services)
        {
            services.AddTransient<IHttpRequestHelper, HttpRequestHelper>();
            services.AddTransient<IJsonHelper, JsonHelper>();
            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
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
            };

            return settings;
        }
    }
}
