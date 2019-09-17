using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using NCS.DSS.EmploymentProgression.Ioc;

[assembly: FunctionsStartup(typeof(Startup))]

namespace NCS.DSS.EmploymentProgression.Ioc
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTriggerSettings();
            services.AddTriggerHelpers();
            services.AddTriggerSupport();
            services.AddTriggerServices();
        }
    }
}
