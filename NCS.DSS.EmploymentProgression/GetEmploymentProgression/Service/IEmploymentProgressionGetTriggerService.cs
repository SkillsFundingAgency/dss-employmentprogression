using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service
{
    public interface IEmploymentProgressionGetTriggerService
    {
        Task<List<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId);
        Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl);
    }
}