namespace NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service
{
    public interface IEmploymentProgressionGetTriggerService
    {
        Task<List<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId);
    }
}