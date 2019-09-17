using System;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.ServiceBus
{
    public interface IServiceBusClient
    {
        Task SendPatchMessageAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl);
        Task SendPostMessageAsync(Models.EmploymentProgression employmentProgression, string reqUrl);
    }
}