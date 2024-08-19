using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.ServiceBus;
using System.Net;

namespace NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service
{
    public class EmploymentProgressionPatchTriggerService : IEmploymentProgressionPatchTriggerService
    {
        private readonly IJsonHelper _jsonHelper;
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public EmploymentProgressionPatchTriggerService(IJsonHelper jsonHelper, IDocumentDBProvider documentDbProvider, IServiceBusClient serviceBusClient)
        {
            _jsonHelper = jsonHelper;
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<string> GetEmploymentProgressionForCustomerToPatchAsync(Guid customerId, Guid employmentProgressionId)
        {
            var employmentProgressionAsString = await _documentDbProvider.GetEmploymentProgressionForCustomerToPatchAsync(customerId, employmentProgressionId);

            return employmentProgressionAsString;
        }

        public async Task<Models.EmploymentProgression> UpdateCosmosAsync(string employmentProgressionAsJson, Guid employmentProgressionId)
        {
            if (string.IsNullOrEmpty(employmentProgressionAsJson))
            {
                return null;
            }

            var response = await _documentDbProvider.UpdateEmploymentProgressionAsync(employmentProgressionAsJson, employmentProgressionId);
            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl, Guid correlationId, ILogger log)
        {
            await _serviceBusClient.SendPatchMessageAsync(employmentProgression, customerId, reqUrl, correlationId, log);
        }

        public bool DoesEmploymentProgressionExistForCustomer(Guid customerId)
        {
            return _documentDbProvider.DoesEmploymentProgressionExistForCustomer(customerId);
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerResourceExist(customerId);
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
