namespace NCS.DSS.EmploymentProgression.Models
{
    public class EmploymentProgressionConfigurationSettings
    {
        public string CosmosDBConnectionString { get; set; }
        public string KeyName { get; internal set; }
        public string AccessKey { get; internal set; }
        public string BaseAddress { get; internal set; }
        public string QueueName { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string AzureMapURL { get; set; }
        public string AzureMapApiVersion { get; set; }
        public string AzureMapSubscriptionKey { get; set; }
        public string AzureCountrySet { get; set; }
        public string DatabaseId { get; internal set; }
        public string CollectionId { get; internal set; }
        public string CustomerDatabaseId { get; internal set; }
        public string CustomerCollectionId { get; internal set; }
    }
}