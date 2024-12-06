using Microsoft.Extensions.Logging;

namespace NCS.DSS.EmploymentProgression.ServiceBus
{
    public interface IEmploymentProgressionServiceBusClient
    {
        Task SendPatchMessageAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl);
        Task SendPostMessageAsync(Models.EmploymentProgression employmentProgression, string reqUrl);
    }
}