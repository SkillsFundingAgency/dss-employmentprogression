using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.EmploymentProgression.Models;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.EmploymentProgression.Cosmos.Provider
{
    public class CosmosDBProvider : ICosmosDBProvider
    {
        private readonly Container _container;
        private readonly Container _customerContainer;
        private readonly ILogger<CosmosDBProvider> _logger;
        public CosmosDBProvider(CosmosClient cosmosClient,
            IOptions<EmploymentProgressionConfigurationSettings> configOptions,
            ILogger<CosmosDBProvider> logger)
        {
            _container = GetContainer(cosmosClient,configOptions.Value.DatabaseId, configOptions.Value.CollectionId);
            _customerContainer = GetContainer(cosmosClient, configOptions.Value.CustomerDatabaseId, configOptions.Value.CustomerCollectionId);
            _logger = logger;
        }
        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
           => cosmosClient.GetContainer(databaseId, collectionId);
        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                var queryCust = _customerContainer.GetItemLinqQueryable<Models.Customer>().Where(x => x.id == customerId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    if (response.Count > 0)
                    {
                        _logger.LogInformation("Customer Record found in Cosmos DB for {CustomerID}", customerId);
                        return true;
                    }
                }
                _logger.LogWarning("No Customer Record found with {CustomerID} in Cosmos DB", customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Customer Record in Cosmos DB {CustomerID}. Exception {Exception}.", customerId, ce.Message);
                throw;
            }
        }
        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            try
            {
                var queryCust = _customerContainer.GetItemLinqQueryable<Models.Customer>().Where(x => x.id == customerId).ToFeedIterator();

                while (queryCust.HasMoreResults)
                {
                    var response = await queryCust.ReadNextAsync();
                    var tDate = response.Resource.FirstOrDefault().DateOfTermination;
                    _logger.LogInformation("Customer with {CustomerID} Have a termination date of {tDate} ", customerId, tDate);
                    return tDate.HasValue;
                }
                _logger.LogWarning("No Customer Record found with {CustomerID} in Cosmos DB", customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to get DateOfTermination for {CustomerID}. Exception {Exception}.", customerId, ce.Message);
                throw;
            }
        }

        public async Task<bool> DoesEmploymentProgressionExistForCustomer(Guid customerId)
        {
            try
            {
                var queryep = _container.GetItemLinqQueryable<Models.EmploymentProgression>().Where(x => x.CustomerId == customerId).ToFeedIterator();

                while (queryep.HasMoreResults)
                {
                    var response = await queryep.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Employment Progression Record found in Cosmos DB for Customer with ID {CustomerID}", customerId);
                        return true;
                    }
                }
                _logger.LogWarning("No Employment Progression found with {CustomerID} in Cosmos DB", customerId);
                return false;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Employment Progression Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }

        public async Task<Models.EmploymentProgression> GetEmploymentProgressionForCustomerAsync(Guid customerId, Guid employmentProgressionId)
        {
            try
            {
                var queryep = _container.GetItemLinqQueryable<Models.EmploymentProgression>()
                                    .Where(x => x.CustomerId == customerId && x.EmploymentProgressionId == employmentProgressionId).ToFeedIterator();

                while (queryep.HasMoreResults)
                {
                    var response = await queryep.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Employment Progression Record found with ID {eProgression} in Cosmos DB for Customer with ID {CustomerID}", employmentProgressionId, customerId);
                        return response.Resource.FirstOrDefault();
                    }
                }
                _logger.LogWarning("No Employment Progression found with ID {eProgression} and Customer ID {CustomerID} in Cosmos DB",employmentProgressionId, customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Employment Progression Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }

        public async Task<IList<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId)
        {
            try
            {
                var queryep = _container.GetItemLinqQueryable<Models.EmploymentProgression>().Where(x => x.CustomerId == customerId).ToFeedIterator();

                while (queryep.HasMoreResults)
                {
                    var response = await queryep.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        _logger.LogInformation("Employment Progression Records found in Cosmos DB for Customer with ID {CustomerID}", customerId);
                        return response.Resource.ToList();
                    }
                }
                _logger.LogWarning("No Employment Progression found with {CustomerID} in Cosmos DB", customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Employment Progression Record in Cosmos DB {CustomerID}. Exception {Exception}", customerId, ce.Message);
                throw;
            }
        }

        public async Task<ItemResponse<Models.EmploymentProgression>> CreateEmploymentProgressionAsync(Models.EmploymentProgression employmentProgression)
        {
            try
            {
                var response = await _container.CreateItemAsync(employmentProgression, null);
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    _logger.LogInformation("Employment Progression Record Created in Cosmos DB for {EmploymentProgressionId}", employmentProgression.EmploymentProgressionId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to Create Employment Progression Record in Cosmos DB for {EmploymentProgressionId}", response.StatusCode, employmentProgression.EmploymentProgressionId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to Create Employment Progression Record in Cosmos DB {EmploymentProgressionId}. Exception {Exception}.", employmentProgression.EmploymentProgressionId, ce.Message);
                throw;
            }
        }

        public async Task<ItemResponse<Models.EmploymentProgression>> UpdateEmploymentProgressionAsync(string employmentProgressionJson, Guid employmentProgressionId)
        {
            try
            {
                var empProg = JsonSerializer.Deserialize<Models.EmploymentProgression>(employmentProgressionJson);
                var response = await _container.ReplaceItemAsync(empProg, employmentProgressionId.ToString());
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("Employment Progression Record Updated in Cosmos DB for {EmploymentProgressionId}", empProg.EmploymentProgressionId);
                }
                else
                {
                    _logger.LogError("Failed and returned {StatusCode} to Update Employment Progression Record in Cosmos DB for {EmploymentProgressionId}", response.StatusCode, empProg.EmploymentProgressionId);
                }
                return response;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to Update Employment Progression Record in Cosmos DB {EmploymentProgressionId}. Exception {Exception}.", employmentProgressionId, ce.Message);
                throw;
            }
        }

        public async Task<string> GetEmploymentProgressionForCustomerToPatchAsync(Guid customerId, Guid employmentProgressionId)
        {
            try
            {
                var queryep = _container.GetItemLinqQueryable<Models.EmploymentProgression>()
                    .Where(x => x.CustomerId == customerId && x.EmploymentProgressionId == employmentProgressionId).ToFeedIterator();

                while (queryep.HasMoreResults)
                {
                    var response = await queryep.ReadNextAsync();
                    if (response != null && response.Resource.Any())
                    {
                        var jsonString = JsonSerializer.Serialize(response.Resource.FirstOrDefault());
                        _logger.LogInformation("Employment Progression Record with {EmploymentProgressionId} found in Cosmos DB for Customer with ID {CustomerID}",employmentProgressionId, customerId);
                        return jsonString;
                    }
                }
                _logger.LogWarning("No Employment Progression with {EmploymentProgressionId} found with {CustomerID} in Cosmos DB",employmentProgressionId, customerId);
                return null;
            }
            catch (CosmosException ce)
            {
                _logger.LogError(ce, "Failed to find the Employment Progression Record with {EmploymentProgressionId} in Cosmos DB for Customer with {CustomerID}. Exception {Exception}",employmentProgressionId, customerId, ce.Message);
                throw;
            }
        }
    }
}