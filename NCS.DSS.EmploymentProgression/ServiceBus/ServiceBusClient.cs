using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NCS.DSS.EmploymentProgression.Models;
using Microsoft.Azure.ServiceBus;
using NCS.DSS.employmentProgression.ServiceBus;

namespace NCS.DSS.EmploymentProgression.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        private readonly ConfigurationSettings _employmentProgressionConfigurationSettings;
        private readonly QueueClient _queueClient;

        public ServiceBusClient(ConfigurationSettings EmploymentProgressionConfigurationSettings)
        {
            _employmentProgressionConfigurationSettings = EmploymentProgressionConfigurationSettings;
            _queueClient = new QueueClient(_employmentProgressionConfigurationSettings.ServiceBusConnectionString, _employmentProgressionConfigurationSettings.QueueName);
        }

        public async Task SendPostMessageAsync(Models.EmploymentProgression employmentProgression, string reqUrl)
        {
            var messageModel = new MessageModel()
            {
                TitleMessage = $"New Employment Progression record {{{employmentProgression.EmploymentProgressionId}}} added at {DateTime.UtcNow}",
                CustomerGuid = employmentProgression.CustomerId,
                LastModifiedDate = employmentProgression.LastModifiedDate,
                URL = reqUrl + "/" + employmentProgression.EmploymentProgressionId,
                IsNewCustomer = false,
                TouchpointId = employmentProgression.LastModifiedTouchpointID
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = $"{employmentProgression.CustomerId} {DateTime.UtcNow}"
            };

            await _queueClient.SendAsync(msg);
        }

        public async Task SendPatchMessageAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl)
        {
            var messageModel = new MessageModel
            {
                TitleMessage = $"Employment Progression record modification for {{{customerId}}} at {DateTime.UtcNow}",
                CustomerGuid = customerId,
                LastModifiedDate = employmentProgression.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = employmentProgression.LastModifiedTouchpointID
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = $"{customerId} {DateTime.UtcNow}"
            };

            await _queueClient.SendAsync(msg);
        }
    }
}
