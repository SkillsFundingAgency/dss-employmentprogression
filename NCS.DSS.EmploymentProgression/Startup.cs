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
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<IEmploymentProgressionPostTriggerService, EmploymentProgressionPostTriggerService>();
            builder.Services.AddTransient<IEmploymentProgressionPatchTriggerService, EmploymentProgressionPatchTriggerService>();
            builder.Services.AddTransient<IEmploymentProgressionGetTriggerService, EmploymentProgressionGetTriggerService>();
            builder.Services.AddTransient<IEmploymentProgressionGetByIdTriggerService, EmploymentProgressionGetByIdTriggerService>();
            builder.Services.AddTransient<IEmploymentProgressionPatchService, EmploymentProgressionPatchService>();

            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();
            builder.Services.AddTransient<IServiceBusClient, ServiceBusClient>();
            builder.Services.AddTransient<IValidate, Validate>();
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddScoped<IGeoCodingService, GeoCodingService>();
            builder.Services.AddScoped<IAzureMapService, AzureMapService>();

            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
            builder.Services.AddSingleton<IResourceHelper, ResourceHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddTransient<IGuidHelper, GuidHelper>();

            var settings = GetConfigurationSettings();
            builder.Services.AddSingleton(settings);
            builder.Services.AddSingleton<CosmosDocumentClient.ICosmosDocumentClient, CosmosDocumentClient.CosmosDocumentClient>(x => new CosmosDocumentClient.CosmosDocumentClient(settings.CosmosDBConnectionString));
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
