using DFC.Common.Standard.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NSubstitute;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using DFC.JSON.Standard;
using System;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;


namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests.Builders
{
    public class EmploymentProgressionGetByIdTriggerBuilder
    {
        public Models.EmploymentProgression EmploymentProgression;
        public ILoggerHelper LoggerHelper { get; set; }
        public IGeoCodingService GeoService { get; set; }
        public JsonHelper JsonHelper { get; set; }
        public HttpResponseMessageHelper ResponseMessageHelper { get; set; }
        public IHttpRequestHelper RequestHelper { get; set; }
        public IEmploymentProgressionGetByIdTriggerService EmploymentProgressionGetByIdTriggerService { get; set; }
        public IResourceHelper ResourceHelper { get; set; }

        public EmploymentProgressionGetByIdTriggerBuilder()
        {
            LoggerHelper = Substitute.For<ILoggerHelper>();
            GeoService = Substitute.For<IGeoCodingService>();
            JsonHelper = new JsonHelper();
            ResponseMessageHelper = new HttpResponseMessageHelper();

            RequestHelper = Substitute.For<IHttpRequestHelper>();
            EmploymentProgression = new Models.EmploymentProgression();
            EmploymentProgressionGetByIdTriggerService = Substitute.For<IEmploymentProgressionGetByIdTriggerService>();
            ResourceHelper = Substitute.For<IResourceHelper>();
        }

        public EmploymentProgressionGetByIdTrigger Build()
        {
            return new EmploymentProgressionGetByIdTrigger(ResponseMessageHelper, RequestHelper, EmploymentProgressionGetByIdTriggerService,
                    JsonHelper, ResourceHelper, LoggerHelper);
        }

        public EmploymentProgressionGetByIdTriggerBuilder WithTouchPointId(string touchpointId)
        {
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns(touchpointId);
            return this;
        }

        public EmploymentProgressionGetByIdTriggerBuilder WithDssApimUrl(string DssApimUrl)
        {
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns(DssApimUrl);
            return this;
        }

        public EmploymentProgressionGetByIdTriggerBuilder WithCustomerExist(bool customerExist)
        {
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(customerExist);
            return this;
        }

        public EmploymentProgressionGetByIdTriggerBuilder WithEmploymentProgressionsForCustomer(Models.EmploymentProgression employmentProgressions)
        {
            EmploymentProgressionGetByIdTriggerService.GetEmploymentProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(employmentProgressions);
            return this;
        }
    }
}
