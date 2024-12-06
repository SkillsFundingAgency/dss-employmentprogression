using DFC.GeoCoding.Standard.AzureMaps.Model;
using Microsoft.Extensions.Logging;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.ReferenceData;
using NCS.DSS.EmploymentProgression.ServiceBus;
using System.Net;

namespace NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service
{
    public class EmploymentProgressionPostTriggerService : IEmploymentProgressionPostTriggerService
    {
        private readonly ICosmosDBProvider _documentDbProvider;
        private readonly IEmploymentProgressionServiceBusClient _serviceBusClient;

        public EmploymentProgressionPostTriggerService(ICosmosDBProvider documentDbProvider, IEmploymentProgressionServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Models.EmploymentProgression> CreateEmploymentProgressionAsync(Models.EmploymentProgression employmentProgression)
        {
            if (employmentProgression == null)
            {
                return null;
            }

            employmentProgression.EmploymentProgressionId = Guid.NewGuid();

            if (!employmentProgression.LastModifiedDate.HasValue)
            {
                employmentProgression.LastModifiedDate = DateTime.UtcNow;
            }

            var response = await _documentDbProvider.CreateEmploymentProgressionAsync(employmentProgression);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(employmentProgression, reqUrl);
        }

        public async Task<bool> DoesEmploymentProgressionExistForCustomer(Guid customerId)
        {
            return await _documentDbProvider.DoesEmploymentProgressionExistForCustomer(customerId);
        }

        public void SetIds(Models.EmploymentProgression employmentProgression, Guid customerGuid, string touchpointId)
        {
            employmentProgression.EmploymentProgressionId = Guid.NewGuid();
            employmentProgression.CustomerId = customerGuid;
            employmentProgression.LastModifiedTouchpointId = touchpointId;
        }

        public void SetDefaults(Models.EmploymentProgression employmentProgression, string touchpointId)
        {
            employmentProgression.CreatedBy = touchpointId;
            employmentProgression.DateProgressionRecorded ??= DateTime.UtcNow;
            employmentProgression.EconomicShockStatus ??= EconomicShockStatus.NotApplicable;
        }

        public void SetLongitudeAndLatitude(Models.EmploymentProgression employmentProgressionRequest, Position position)
        {
            if (position == null || employmentProgressionRequest == null)
            {
                return;
            }

            employmentProgressionRequest.Longitude = (decimal)position.Lon;
            employmentProgressionRequest.Latitude = (decimal)position.Lat;
        }
    }
}
