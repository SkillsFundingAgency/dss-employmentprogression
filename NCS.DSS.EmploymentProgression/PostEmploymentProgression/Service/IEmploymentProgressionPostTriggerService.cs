using DFC.GeoCoding.Standard.AzureMaps.Model;

namespace NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service
{
    public interface IEmploymentProgressionPostTriggerService
    {
        Task<Models.EmploymentProgression> CreateEmploymentProgressionAsync(Models.EmploymentProgression employmentProgression);
        Task SendToServiceBusQueueAsync(Models.EmploymentProgression employmentProgression, string reqUrl);
        Task<bool> DoesEmploymentProgressionExistForCustomer(Guid customerId);
        void SetIds(Models.EmploymentProgression employmentProgression, Guid customerGuid, string touchpointId);
        void SetLongitudeAndLatitude(Models.EmploymentProgression employmentProgressionRequest, Position position);
        void SetDefaults(Models.EmploymentProgression employmentProgression, string touchpointId);
    }
}