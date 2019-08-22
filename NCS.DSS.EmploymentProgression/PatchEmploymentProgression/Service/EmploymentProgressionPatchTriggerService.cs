using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.JSON.Standard;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

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

        public string PatchEmploymentProgressionAsync(string employmentProgressionAsJson, EmploymentProgressionPatch employmentProgressionPatch)
        {
            try
            {
                var employmentProgressionAsJsonObject = JObject.Parse(employmentProgressionAsJson);

                if (employmentProgressionPatch.DateProgressionRecorded.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["DateProgressionRecorded"], employmentProgressionPatch.DateProgressionRecorded);

                if (employmentProgressionPatch.CurrentEmploymentStatus.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["CurrentEmploymentStatus"], employmentProgressionPatch.CurrentEmploymentStatus);

                if (employmentProgressionPatch.EconomicShockStatus.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EconomicShockStatus"], employmentProgressionPatch.EconomicShockStatus);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.EconomicShockCode))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EconomicShockCode"], employmentProgressionPatch.EconomicShockCode);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.EmployerName))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EmployerName"], employmentProgressionPatch.EmployerName);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.EmployerAddress))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EmployerAddress"], employmentProgressionPatch.EmployerAddress);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.EmployerPostcode))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EmployerPostcode"], employmentProgressionPatch.EmployerPostcode);

                if (employmentProgressionPatch.Latitude.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["Latitude"], employmentProgressionPatch.Latitude);

                if (employmentProgressionPatch.Longitude.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["Longitude"], employmentProgressionPatch.Longitude);

                if (employmentProgressionPatch.DateOfEmployment.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["DateOfEmployment"], employmentProgressionPatch.DateOfEmployment);

                if (employmentProgressionPatch.DateOfLastEmployment.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["DateOfLastEmployment"], employmentProgressionPatch.DateOfLastEmployment);

                if (employmentProgressionPatch.LengthOfUnemployment.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["LengthOfUnemployment"], employmentProgressionPatch.LengthOfUnemployment);

                if (employmentProgressionPatch.LastModifiedDate.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["LastModifiedDate"], employmentProgressionPatch.LastModifiedDate);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.LastModifiedTouchpointId))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["LastModifiedTouchpointID"], employmentProgressionPatch.LastModifiedTouchpointId);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.CreatedBy))
                {
                    if (employmentProgressionAsJsonObject["CreatedBy"] == null)
                        _jsonHelper.CreatePropertyOnJObject(employmentProgressionAsJsonObject, "CreatedBy", employmentProgressionPatch.CreatedBy);
                    else
                        _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["CreatedBy"], employmentProgressionPatch.CreatedBy);
                }

                return employmentProgressionAsJsonObject.ToString();
            }
            catch (JsonReaderException)
            {
                return null;
            }
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

        public async Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(employmentProgression, reqUrl);
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

        public void SetLongitudeAndLatitude(EmploymentProgressionPatch employmentProgressionPatchRequest, Position position)
        {
            if (position == null || employmentProgressionPatchRequest == null)
            {
                return;
            }

            employmentProgressionPatchRequest.Longitude = position.Lon;
            employmentProgressionPatchRequest.Latitude = position.Lat;
        }
    }
}
