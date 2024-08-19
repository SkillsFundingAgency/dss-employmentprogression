using DFC.Common.Standard.Logging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.EmploymentProgression.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.EmploymentProgression.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        private readonly ConfigurationSettings _employmentProgressionConfigurationSettings;
        private readonly QueueClient _queueClient;
        private readonly ILoggerHelper _loggerHelper = new LoggerHelper();

        public ServiceBusClient(ConfigurationSettings EmploymentProgressionConfigurationSettings)
        {
            _employmentProgressionConfigurationSettings = EmploymentProgressionConfigurationSettings;
            _queueClient = new QueueClient(_employmentProgressionConfigurationSettings.ServiceBusConnectionString, _employmentProgressionConfigurationSettings.QueueName);
        }

        public async Task SendPostMessageAsync(Models.EmploymentProgression employmentProgression, string reqUrl, Guid correlationId, ILogger log)
        {
            var messageModel = new MessageModel()
            {
                TitleMessage = $"New Employment Progression record {{{employmentProgression.EmploymentProgressionId}}} added at {DateTime.UtcNow}",
                CustomerGuid = employmentProgression.CustomerId,
                LastModifiedDate = employmentProgression.LastModifiedDate,
                URL = reqUrl + "/" + employmentProgression.EmploymentProgressionId,
                IsNewCustomer = false,
                TouchpointId = employmentProgression.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = $"{employmentProgression.CustomerId} {DateTime.UtcNow}"
            };
            //Messages now logged to appinsights
            _loggerHelper.LogInformationObject(log, correlationId, string.Format("New Employment Progression record {0}", employmentProgression.EmploymentProgressionId), messageModel);

            await _queueClient.SendAsync(msg);
        }

        public async Task SendPatchMessageAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl, Guid correlationId, ILogger log)
        {
            var messageModel = new MessageModel
            {
                TitleMessage = $"Employment Progression record modification for {{{customerId}}} at {DateTime.UtcNow}",
                CustomerGuid = customerId,
                LastModifiedDate = employmentProgression.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = employmentProgression.LastModifiedTouchpointId
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = $"{customerId} {DateTime.UtcNow}"
            };

            _loggerHelper.LogInformationObject(log, correlationId, $"Employment Progression record modification for {{{customerId}}} at {DateTime.UtcNow}", messageModel);
            await _queueClient.SendAsync(msg);
        }
    }
}
