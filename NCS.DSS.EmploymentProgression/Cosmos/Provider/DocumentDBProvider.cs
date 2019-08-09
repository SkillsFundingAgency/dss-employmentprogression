using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.CosmosDocumentClient;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.EmploymentProgression.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly ICosmosDocumentClient _cosmosDocumentClient;

        public DocumentDBProvider(ICosmosDocumentClient cosmosDocumentClient)
        {
            _cosmosDocumentClient = cosmosDocumentClient;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBUrlHelper.CreateCustomerDocumentUri(customerId);

            try
            {
                var client = _cosmosDocumentClient.GetDocumentClient();
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                {
                    return true;
                }
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBUrlHelper.CreateCustomerDocumentUri(customerId);

            try
            {
                var client = _cosmosDocumentClient.GetDocumentClient();
                var response = await client.ReadDocumentAsync(documentUri);
                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }

        public bool DoesEmploymentProgressionExistForCustomer(Guid customerId)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                return false;
            }

            var employmentProgressionForCustomerQuery = client.CreateDocumentQuery<Models.EmploymentProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 });
            var result = employmentProgressionForCustomerQuery.Where(x => x.CustomerId == customerId).AsEnumerable().Any();

            return result;
        }

        public async Task<Models.EmploymentProgression> GetEmploymentProgressionForCustomerAsync(Guid customerId, Guid employmentProgressionId)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            var employmentProgressionForCustomerQuery = client
                ?.CreateDocumentQuery<Models.EmploymentProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.EmploymentProgressionId == employmentProgressionId)
                .AsDocumentQuery();

            if (employmentProgressionForCustomerQuery == null)
            {
                return null;
            }

            var employmentProgression = await employmentProgressionForCustomerQuery.ExecuteNextAsync<Models.EmploymentProgression>();

            return employmentProgression?.FirstOrDefault();
        }

        public async Task<List<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();

            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            {
                return null;
            }

            var employmentProgressionsQuery = client.CreateDocumentQuery<Models.EmploymentProgression>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var employmentProgressions = new List<Models.EmploymentProgression>();

            while (employmentProgressionsQuery.HasMoreResults)
            {
                var response = await employmentProgressionsQuery.ExecuteNextAsync<Models.EmploymentProgression>();
                employmentProgressions.AddRange(response);
            }

            return employmentProgressions.Any() ? employmentProgressions : null;
        }

        public async Task<ResourceResponse<Document>> CreateEmploymentProgressionAsync(Models.EmploymentProgression employmentProgression)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();

            var client = _cosmosDocumentClient.GetDocumentClient();
            var response = await client.CreateDocumentAsync(collectionUri, employmentProgression);

            return response;
        }

        public async Task<ResourceResponse<Document>> UpdateEmploymentProgressionAsync(string employmentProgressionJson, Guid employmentProgressionId)
        {
            if (string.IsNullOrEmpty(employmentProgressionJson))
            {
                return null;
            }

            var documentUri = DocumentDBUrlHelper.CreateDocumentUri(employmentProgressionId);
            var client = _cosmosDocumentClient.GetDocumentClient();

            if (client == null)
            { 
                return null;
            }

            var employmentProgressionDocumentJObject = JObject.Parse(employmentProgressionJson);
            var response = await client.ReplaceDocumentAsync(documentUri, employmentProgressionDocumentJObject);

            return response;
        }

        public async Task<string> GetEmploymentProgressionForCustomerToPatchAsync(Guid customerId, Guid employmentProgressionId)
        {
            var collectionUri = DocumentDBUrlHelper.CreateDocumentCollectionUri();
            var client = _cosmosDocumentClient.GetDocumentClient();

            var employmentProgressionQuery = client
                ?.CreateDocumentQuery<Models.EmploymentProgression>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.EmploymentProgressionId == employmentProgressionId)
                .AsDocumentQuery();

            if (employmentProgressionQuery == null)
            {
                return null;
            }

            var employmentProgressions = await employmentProgressionQuery.ExecuteNextAsync();
            return employmentProgressions?.FirstOrDefault()?.ToString();
        }
    }
}