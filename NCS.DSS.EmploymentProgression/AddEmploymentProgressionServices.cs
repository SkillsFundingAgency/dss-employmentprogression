using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;

namespace NCS.DSS.EmploymentProgression
{
    public static class AddEmploymentProgressionServices
    {
        public static IServiceCollection AddTriggerServices(this IServiceCollection services)
        {
            services.AddTransient<IEmploymentProgressionPostTriggerService, EmploymentProgressionPostTriggerService>();
            services.AddTransient<IEmploymentProgressionPatchTriggerService, EmploymentProgressionPatchTriggerService>();
            services.AddTransient<IEmploymentProgressionGetTriggerService, EmploymentProgressionGetTriggerService>();
            services.AddTransient<IEmploymentProgressionGetByIdTriggerService, EmploymentProgressionGetByIdTriggerService>();

            return services;
        }
    }
}
