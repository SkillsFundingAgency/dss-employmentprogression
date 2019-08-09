using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.ServiceBus;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service
{
    public class EmploymentProgressionGetByIdService : IEmploymentProgressionGetByIdService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public EmploymentProgressionGetByIdService(IDocumentDBProvider documentDbProvider, IServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Models.EmploymentProgression> GetEmploymentProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId)
        {
            return await _documentDbProvider.GetEmploymentProgressionForCustomerAsync(customerId, progressionProgressionId);
        }

        public async Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(employmentProgression, reqUrl);
        }
    }
}
