using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.EmploymentProgression.ServiceBus
{
    public class EmploymentProgressionServiceBusClient : IEmploymentProgressionServiceBusClient
    {
        private readonly ILogger<EmploymentProgressionServiceBusClient> _logger;
        public readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        private readonly ServiceBusClient _serviceBusClient;
        public EmploymentProgressionServiceBusClient(ServiceBusClient serviceBusClient, ILogger<EmploymentProgressionServiceBusClient> logger)
        {
           _serviceBusClient = serviceBusClient;
            _logger = logger;
        }

        public async Task SendPostMessageAsync(Models.EmploymentProgression employmentProgression, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Attempting to Create Sender for Service Bus Client");
                var serviceBusSender = _serviceBusClient.CreateSender(QueueName);
                _logger.LogInformation("Preparing Message for Service Bus");
                var messageModel = new MessageModel()
                {
                    TitleMessage = $"New Employment Progression record {{{employmentProgression.EmploymentProgressionId}}} added at {DateTime.UtcNow}",
                    CustomerGuid = employmentProgression.CustomerId,
                    LastModifiedDate = employmentProgression.LastModifiedDate,
                    URL = reqUrl + "/" + employmentProgression.EmploymentProgressionId,
                    IsNewCustomer = false,
                    TouchpointId = employmentProgression.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = $"{employmentProgression.CustomerId} {DateTime.UtcNow}"
                };
                _logger.LogInformation("Attempting to Send Service Bus Message for Employment Progression with ID {EmploymentProgressionId}", employmentProgression.EmploymentProgressionId);
                await serviceBusSender.SendMessageAsync(msg);
                _logger.LogInformation("POST Service Bus Message for Employment Progression with ID {EmploymentProgressionId} has been sent successfully", employmentProgression.EmploymentProgressionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Send POST Service Bus Message for Employment Progression with ID {EmploymentProgressionId}. Exception Raised with {Message}.", employmentProgression.EmploymentProgressionId, ex.Message);
                throw;
            }            
        }

        public async Task SendPatchMessageAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Attempting to Create Sender for Service Bus Client");
                var serviceBusSender = _serviceBusClient.CreateSender(QueueName);
                _logger.LogInformation("Preparing Message for Service Bus");
                var messageModel = new MessageModel
                {
                    TitleMessage = $"Employment Progression record modification for {{{customerId}}} at {DateTime.UtcNow}",
                    CustomerGuid = customerId,
                    LastModifiedDate = employmentProgression.LastModifiedDate,
                    URL = reqUrl,
                    IsNewCustomer = false,
                    TouchpointId = employmentProgression.LastModifiedTouchpointId
                };

                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
                {
                    ContentType = "application/json",
                    MessageId = $"{customerId} {DateTime.UtcNow}"
                };
                _logger.LogInformation("Attempting to Send Service Bus Message for Employment Progression with ID {EmploymentProgressionId}", employmentProgression.EmploymentProgressionId);
                await serviceBusSender.SendMessageAsync(msg);
                _logger.LogInformation("PATCH Service Bus Message for Employment Progression with ID {EmploymentProgressionId} has been sent successfully", employmentProgression.EmploymentProgressionId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Send PATCH Service Bus Message for Employment Progression with ID {EmploymentProgressionId}. Exception Raised with {Message}.", employmentProgression.EmploymentProgressionId, ex.Message);
                throw;
            }
        }
    }
}
