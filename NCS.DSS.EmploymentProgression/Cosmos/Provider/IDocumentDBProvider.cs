using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        bool DoesEmploymentProgressionExistForCustomer(Guid customerId);
        Task<Models.EmploymentProgression> GetEmploymentProgressionForCustomerAsync(Guid customerId, Guid employmentProgressionId);
        Task<List<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId);
        Task<ResourceResponse<Document>> CreateEmploymentProgressionAsync(Models.EmploymentProgression employmentProgression);
        Task<ResourceResponse<Document>> UpdateEmploymentProgressionAsync(string employmentProgressionJson, Guid employmentProgressionId);
        Task<string> GetEmploymentProgressionForCustomerToPatchAsync(Guid customerId, Guid employmentProgressionId);
    }
}