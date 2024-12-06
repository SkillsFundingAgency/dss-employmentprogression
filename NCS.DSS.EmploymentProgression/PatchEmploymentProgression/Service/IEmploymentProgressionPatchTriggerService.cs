using DFC.GeoCoding.Standard.AzureMaps.Model;
using Microsoft.Extensions.Logging;
using NCS.DSS.EmploymentProgression.Models;

namespace NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service
{
    public interface IEmploymentProgressionPatchTriggerService
    {
        Task<Models.EmploymentProgression> UpdateCosmosAsync(string employmentProgressionAsJson, Guid employmentProgressionId);
        Task<string> GetEmploymentProgressionForCustomerToPatchAsync(Guid customerId, Guid employmentProgressionId);
        Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl, Guid correlationId, ILogger log);
        Task<bool> DoesEmploymentProgressionExistForCustomer(Guid customerId);
        Task<bool> DoesCustomerExist(Guid customerId);
        void SetIds(EmploymentProgressionPatch employmentProgressionPatch, Guid employmentProgressionGuid, string touchpointId);
        void SetDefaults(EmploymentProgressionPatch employmentProgressionPatch);
        void SetLongitudeAndLatitude(EmploymentProgressionPatch employmentProgressionPatchRequest, Position position);
    }
}