using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.CosmosDocumentClient;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.ServiceBus;
using NCS.DSS.EmploymentProgression.Validators;
using System;
var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
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

        services.AddSingleton(settings);
        services.AddSingleton<ICosmosDocumentClient, CosmosDocumentClient>(x => new CosmosDocumentClient(settings.CosmosDBConnectionString));

        services.AddTransient<IEmploymentProgressionPostTriggerService, EmploymentProgressionPostTriggerService>();
        services.AddTransient<IEmploymentProgressionPatchTriggerService, EmploymentProgressionPatchTriggerService>();
        services.AddTransient<IEmploymentProgressionGetTriggerService, EmploymentProgressionGetTriggerService>();
        services.AddTransient<IEmploymentProgressionGetByIdTriggerService, EmploymentProgressionGetByIdTriggerService>();
        services.AddTransient<IEmploymentProgressionPatchService, EmploymentProgressionPatchService>();

        services.AddSingleton(typeof(IConvertToDynamic<>), typeof(ConvertToDynamic<>));
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
    })
    .Build();

host.Run();
