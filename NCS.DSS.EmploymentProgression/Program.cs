using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using DFC.GeoCoding.Standard.OrdnanceSurvey.Models;
using DFC.GeoCoding.Standard.OrdnanceSurvey.Services;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.ServiceBus;
using NCS.DSS.EmploymentProgression.Validators;

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
                    services.Configure<OSServiceOptions>(options =>
                    {
                        var settings = configuration.Get<EmploymentProgressionConfigurationSettings>();
                        options.ApiUrl = settings.OSServiceApiUrl;
                        options.ApiKey = settings.OSServiceApiKey;
                    });

                    services.AddHttpClient<IOSService, OSService>();
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
                        var logger = sp.GetRequiredService<ILogger<Program>>();

                        var connectionString = configuration["CosmosDBConnectionString"];
                        var endpoint = configuration["CosmosDbEndpoint"];

                        var options = new CosmosClientOptions
                        {
                            ConnectionMode = ConnectionMode.Gateway
                        };

                        if (!string.IsNullOrWhiteSpace(endpoint))
                        {
                            logger.LogTrace("Using DefaultAzureCredential for Cosmos DB (managed identity)");
                            return new CosmosClient(endpoint, new DefaultAzureCredential(), options);
                        }
                        else if (!string.IsNullOrWhiteSpace(connectionString))
                        {
                            logger.LogTrace("No managed identity found: using Cosmos DB connection string (local development)");
                            return new CosmosClient(connectionString, options);
                        }
                        else
                        {
                            throw new InvalidOperationException("Neither CosmosDbEndpoint or a ConnectionString are configured");
                        }
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
                        options.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
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