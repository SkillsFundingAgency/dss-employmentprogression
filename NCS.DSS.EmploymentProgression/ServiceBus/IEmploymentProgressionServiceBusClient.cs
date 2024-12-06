using Microsoft.Extensions.Logging;

namespace NCS.DSS.EmploymentProgression.ServiceBus
{
    public interface IEmploymentProgressionServiceBusClient
    {
        Task SendPatchMessageAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl, Guid correlationId, ILogger log);
        Task SendPostMessageAsync(Models.EmploymentProgression employmentProgression, string reqUrl, Guid correlationId, ILogger log);
    }
}