namespace NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service
{
    public interface IEmploymentProgressionGetByIdTriggerService
    {
        Task<Models.EmploymentProgression> GetEmploymentProgressionForCustomerAsync(Guid customerId, Guid progressionProgressionId);
    }
}