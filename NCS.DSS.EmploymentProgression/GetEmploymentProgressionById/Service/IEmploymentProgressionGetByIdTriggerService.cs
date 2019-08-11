using System;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service
{
    public interface IEmploymentProgressionGetByIdTriggerService
    {
        Task<Models.EmploymentProgression> GetEmploymentProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId);
        Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl);
    }
}