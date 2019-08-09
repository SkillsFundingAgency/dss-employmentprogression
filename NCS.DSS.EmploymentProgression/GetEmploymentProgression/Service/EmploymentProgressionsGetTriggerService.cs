using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.ServiceBus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service
{
    public class EmploymentProgressionsGetTriggerService : IEmploymentProgressionsGetTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public EmploymentProgressionsGetTriggerService(IDocumentDBProvider documentDbProvider, IServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<List<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId)
        {
            return await _documentDbProvider.GetEmploymentProgressionsForCustomerAsync(customerId);
        }

        public async Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPostMessageAsync(employmentProgression, reqUrl);
        }
    }
}
