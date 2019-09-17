namespace NCS.DSS.EmploymentProgression.Models
{
    public class ConfigurationSettings
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
    }
}