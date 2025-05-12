using Microsoft.Azure.Cosmos;

namespace NCS.DSS.EmploymentProgression.Cosmos.Provider
{
    public interface ICosmosDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<bool> DoesEmploymentProgressionExistForCustomer(Guid customerId);
        Task<Models.EmploymentProgression> GetEmploymentProgressionForCustomerAsync(Guid customerId, Guid employmentProgressionId);
        Task<IList<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId);
        Task<ItemResponse<Models.EmploymentProgression>> CreateEmploymentProgressionAsync(Models.EmploymentProgression employmentProgression);
        Task<ItemResponse<Models.EmploymentProgression>> UpdateEmploymentProgressionAsync(string employmentProgressionJson, Guid employmentProgressionId);
        Task<string> GetEmploymentProgressionForCustomerToPatchAsync(Guid customerId, Guid employmentProgressionId);
    }
}