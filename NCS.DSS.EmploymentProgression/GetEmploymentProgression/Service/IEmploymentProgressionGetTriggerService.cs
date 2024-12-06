namespace NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service
{
    public interface IEmploymentProgressionGetTriggerService
    {
        Task<IList<Models.EmploymentProgression>> GetEmploymentProgressionsForCustomerAsync(Guid customerId);
    }
}