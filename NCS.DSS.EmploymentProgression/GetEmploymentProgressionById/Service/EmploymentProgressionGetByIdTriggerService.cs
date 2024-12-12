using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.ServiceBus;

namespace NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service
{
    public class EmploymentProgressionGetByIdTriggerService : IEmploymentProgressionGetByIdTriggerService
    {
        private readonly ICosmosDBProvider _cosmosDbProvider;
        private readonly IEmploymentProgressionServiceBusClient _serviceBusClient;

        public EmploymentProgressionGetByIdTriggerService(ICosmosDBProvider cosmosDBProvider, IEmploymentProgressionServiceBusClient serviceBusClient)
        {
            _cosmosDbProvider = cosmosDBProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<Models.EmploymentProgression> GetEmploymentProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId)
        {
            return await _cosmosDbProvider.GetEmploymentProgressionForCustomerAsync(customerId, progressionProgressionId);
        }
    }
}
