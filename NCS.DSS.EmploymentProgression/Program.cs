using Azure.Messaging.ServiceBus;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Functions.Worker;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.ServiceBus;
using NCS.DSS.EmploymentProgression.Validators;
using Microsoft.Extensions.Configuration;
namespace NCS.DSS.EmploymentProgression
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.SetBasePath(Environment.CurrentDirectory)
                        .AddJsonFile("local.settings.json", optional: true,
                            reloadOnChange: false)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((context,services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<EmploymentProgressionConfigurationSettings>()
                        .Bind(configuration);
                    services.AddLogging();
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddTransient<IEmploymentProgressionPostTriggerService, EmploymentProgressionPostTriggerService>();
                    services.AddTransient<IEmploymentProgressionPatchTriggerService, EmploymentProgressionPatchTriggerService>();
                    services.AddTransient<IEmploymentProgressionGetTriggerService, EmploymentProgressionGetTriggerService>();
                    services.AddTransient<IEmploymentProgressionGetByIdTriggerService, EmploymentProgressionGetByIdTriggerService>();
                    services.AddTransient<IEmploymentProgressionPatchService, EmploymentProgressionPatchService>();

                    services.AddSingleton(typeof(IConvertToDynamic<>), typeof(ConvertToDynamic<>));
                    services.AddSingleton<ICosmosDBProvider, CosmosDBProvider>();
                    services.AddSingleton(sp =>
                    {
                        var settings = sp.GetRequiredService<IOptions<EmploymentProgressionConfigurationSettings>>().Value;
                        var options = new CosmosClientOptions()
                        {
                            ConnectionMode = ConnectionMode.Gateway
                        };
                        return new CosmosClient(settings.CosmosDBConnectionString, options);
                    });
                    services.AddScoped<IEmploymentProgressionServiceBusClient, EmploymentProgressionServiceBusClient>();
                    services.AddSingleton(serviceProvider =>
                    {
                        var settings = serviceProvider.GetRequiredService<IOptions<EmploymentProgressionConfigurationSettings>>().Value;
                        return new ServiceBusClient(settings.ServiceBusConnectionString);
                    });
                    
                    services.AddTransient<IValidate, Validate>();
                    services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddScoped<IGeoCodingService, GeoCodingService>();
                    services.AddScoped<IAzureMapService, AzureMapService>();

                    services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddSingleton<IJsonHelper, JsonHelper>();
                    services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });

                })
                .Build();

            await host.RunAsync();
        }
    }
}