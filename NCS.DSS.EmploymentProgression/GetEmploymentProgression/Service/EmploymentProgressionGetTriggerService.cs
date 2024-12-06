using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.ServiceBus;

namespace NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service
{
    public class EmploymentProgressionGetTriggerService : IEmploymentProgressionGetTriggerService
    {
        private readonly ICosmosDBProvider _documentDbProvider;
        private readonly IEmploymentProgressionServiceBusClient _serviceBusClient;

        public EmploymentProgressionGetTriggerService(ICosmosDBProvider documentDbProvider, IEmploymentProgressionServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<IList<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId)
        {
            return await _documentDbProvider.GetEmploymentProgressionsForCustomerAsync(customerId);
        }
    }
}
