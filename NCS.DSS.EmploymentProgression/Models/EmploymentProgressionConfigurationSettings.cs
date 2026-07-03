namespace NCS.DSS.EmploymentProgression.Models
{
    public class EmploymentProgressionConfigurationSettings
    {
        public required string CosmosDbEndpoint { get; set; }
        public string CosmosDBConnectionString { get; set; }
        public string KeyName { get; set; }
        public string AccessKey { get; set; }
        public string BaseAddress { get; set; }
        public string QueueName { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string DatabaseId { get; set; }
        public string CollectionId { get; set; }
        public string CustomerDatabaseId { get; set; }
        public string CustomerCollectionId { get; set; }
        public string OSServiceApiUrl { get; set; }
        public string OSServiceApiKey { get; set; }
    }
}