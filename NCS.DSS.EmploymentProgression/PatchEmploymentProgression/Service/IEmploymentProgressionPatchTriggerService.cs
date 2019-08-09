using NCS.DSS.EmploymentProgression.Models;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service
{
    public interface IEmploymentProgressionPatchTriggerService
    {
        string PatchEmploymentProgressionAsync(string employmentProgressionAsJson, EmploymentProgressionPatch employmentProgressionPatch);
        Task<Models.EmploymentProgression> UpdateCosmosAsync(string employmentProgressionAsJson, Guid employmentProgressionId);
        Task<string> GetEmploymentProgressionForCustomerToPatchAsync(Guid customerId, Guid employmentProgressionId);
        Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, string reqUrl);
        bool DoesEmploymentProgressionExistForCustomer(Guid customerId);
        Task<bool> DoesCustomerExist(Guid customerId);
        void SetIds(EmploymentProgressionPatch employmentProgressionPatch, Guid employmentProgressionGuid, string touchpointId);
    }
}