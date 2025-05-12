using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.ServiceBus;

namespace NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service
{
    public class EmploymentProgressionGetTriggerService : IEmploymentProgressionGetTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IEmploymentProgressionServiceBusClient _serviceBusClient;

        public EmploymentProgressionGetTriggerService(ICosmosDBProvider cosmosDBProvider, IEmploymentProgressionServiceBusClient serviceBusClient)
        {
            _cosmosDbProvider = cosmosDBProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<IList<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId)
        {
            return await _cosmosDbProvider.GetEmploymentProgressionsForCustomerAsync(customerId);
        }
    }
}
