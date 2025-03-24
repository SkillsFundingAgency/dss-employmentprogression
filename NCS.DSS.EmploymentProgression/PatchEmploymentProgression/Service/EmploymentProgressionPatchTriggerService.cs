using DFC.GeoCoding.Standard.AzureMaps.Model;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.ServiceBus;
using System.Net;

namespace NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service
{
    public class EmploymentProgressionPatchTriggerService : IEmploymentProgressionPatchTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IEmploymentProgressionServiceBusClient _serviceBusClient;

        public EmploymentProgressionPatchTriggerService( ICosmosDBProvider cosmosDbProvider, IEmploymentProgressionServiceBusClient serviceBusClient)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<string> GetEmploymentProgressionForCustomerToPatchAsync(Guid customerId, Guid employmentProgressionId)
        {
            var employmentProgressionAsString = await _cosmosDbProvider.GetEmploymentProgressionForCustomerToPatchAsync(customerId, employmentProgressionId);

            return employmentProgressionAsString;
        }

        public async Task<Models.EmploymentProgression> UpdateCosmosAsync(string employmentProgressionAsJson, Guid employmentProgressionId)
        {
            if (string.IsNullOrEmpty(employmentProgressionAsJson))
            {
                return null;
            }

            var response = await _cosmosDbProvider.UpdateEmploymentProgressionAsync(employmentProgressionAsJson, employmentProgressionId);
            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPatchMessageAsync(employmentProgression, customerId, reqUrl);
        }

        public async Task<bool> DoesEmploymentProgressionExistForCustomer(Guid customerId)
        {
            return await _cosmosDbProvider.DoesEmploymentProgressionExistForCustomer(customerId);
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerResourceExist(customerId);
        }

        public void SetIds(EmploymentProgressionPatch employmentProgressionPatch, Guid employmentProgressionGuid, string touchpointId)
        {
            employmentProgressionPatch.LastModifiedTouchpointId = touchpointId;
            employmentProgressionPatch.EmploymentProgressionId = employmentProgressionGuid;
        }

        public void SetDefaults(EmploymentProgressionPatch employmentProgressionPatch)
        {
            if (!employmentProgressionPatch.LastModifiedDate.HasValue)
            {
                employmentProgressionPatch.LastModifiedDate = DateTime.UtcNow;
            }
        }

        public void SetLongitudeAndLatitude(EmploymentProgressionPatch employmentProgressionPatchRequest, Position position)
        {
            if (position == null || employmentProgressionPatchRequest == null)
            {
                return;
            }

            employmentProgressionPatchRequest.Longitude = (decimal)position.Lon;
            employmentProgressionPatchRequest.Latitude = (decimal)position.Lat;
        }
    }
}
