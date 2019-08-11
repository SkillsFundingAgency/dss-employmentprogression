using DFC.Common.Standard.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NSubstitute;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using DFC.JSON.Standard;
using System;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Models;
using System.Collections.Generic;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests.Builders
{
    public class EmploymentProgressionGetTriggerBuilder
    {
        public Models.EmploymentProgression EmploymentProgression;
        public ILoggerHelper LoggerHelper { get; set; }
        public IGeoCodingService GeoService { get; set; }
        public JsonHelper JsonHelper { get; set; }
        public HttpResponseMessageHelper ResponseMessageHelper { get; set; }
        public IHttpRequestHelper RequestHelper { get; set; }
        public IEmploymentProgressionGetTriggerService EmploymentProgressionGetTriggerService { get; set; }
        public IResourceHelper ResourceHelper { get; set; }

        public EmploymentProgressionGetTriggerBuilder()
        {
            LoggerHelper = Substitute.For<ILoggerHelper>();
            GeoService = Substitute.For<IGeoCodingService>();
            JsonHelper = new JsonHelper();
            ResponseMessageHelper = new HttpResponseMessageHelper();

            RequestHelper = Substitute.For<IHttpRequestHelper>();
            EmploymentProgression = new Models.EmploymentProgression();
            EmploymentProgressionGetTriggerService = Substitute.For<IEmploymentProgressionGetTriggerService>();
            ResourceHelper = Substitute.For<IResourceHelper>();
        }

        public EmploymentProgressionGetTrigger Build()
        {
            return new EmploymentProgressionGetTrigger(ResponseMessageHelper, RequestHelper, EmploymentProgressionGetTriggerService,
                    JsonHelper, ResourceHelper, LoggerHelper);
        }

        public EmploymentProgressionGetTriggerBuilder WithTouchPointId(string touchpointId)
        {
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns(touchpointId);
            return this;
        }

        public EmploymentProgressionGetTriggerBuilder WithDssApimUrl(string DssApimUrl)
        {
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns(DssApimUrl);
            return this;
        }

        public EmploymentProgressionGetTriggerBuilder WithCustomerExist(bool customerExist)
        {
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(customerExist);
            return this;
        }

        public EmploymentProgressionGetTriggerBuilder WithEmploymentProgressionsForCustomer(List<Models.EmploymentProgression> employmentProgressions)
        {
            EmploymentProgressionGetTriggerService.GetEmploymentProgressionsForCustomerAsync(Arg.Any<Guid>()).Returns(employmentProgressions);
            return this;
        }
    }
}
