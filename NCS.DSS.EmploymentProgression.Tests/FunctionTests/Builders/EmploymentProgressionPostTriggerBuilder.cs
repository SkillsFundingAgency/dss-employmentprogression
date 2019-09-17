using DFC.Common.Standard.Logging;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.Function;
using NCS.DSS.EmploymentProgression.Validators;
using NSubstitute;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using DFC.JSON.Standard;
using System;
using Newtonsoft.Json;
using NSubstitute.ExceptionExtensions;
using DFC.Common.Standard.GuidHelper;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests.Builders
{
    public class EmploymentProgressionPostTriggerBuilder
    {
        public Models.EmploymentProgression EmploymentProgression;
        public ILoggerHelper LoggerHelper { get; set; }
        public IGeoCodingService GeoService { get; set; }
        public JsonHelper JsonHelper { get; set; }
        public GuidHelper GuidHelper { get; set; }
        public HttpResponseMessageHelper ResponseMessageHelper { get; set; }
        public IHttpRequestHelper RequestHelper { get; set; }
        public IEmploymentProgressionPostTriggerService EmploymentProgressionPostTriggerService { get; set; }
        public IResourceHelper ResourceHelper { get; set; }
        public IValidate Valdiator { get; set; }

        public EmploymentProgressionPostTriggerBuilder()
        {
            LoggerHelper = Substitute.For<ILoggerHelper>();
            GeoService = Substitute.For<IGeoCodingService>();
            JsonHelper = new JsonHelper();
            GuidHelper = new GuidHelper();
            ResponseMessageHelper = new HttpResponseMessageHelper();

            RequestHelper = Substitute.For<IHttpRequestHelper>();
            EmploymentProgression = new Models.EmploymentProgression();
            EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            ResourceHelper = Substitute.For<IResourceHelper>();
            Valdiator = Substitute.For<IValidate>();
        }

        public EmploymentProgressionPostTrigger Build()
        {
            return new EmploymentProgressionPostTrigger(ResponseMessageHelper, RequestHelper, EmploymentProgressionPostTriggerService,
                    JsonHelper, ResourceHelper, Valdiator, LoggerHelper, GeoService, GuidHelper);
        }

        public EmploymentProgressionPostTriggerBuilder WithTouchPointId(string touchpointId)
        {
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns(touchpointId);
            return this;
        }

        public EmploymentProgressionPostTriggerBuilder WithDssApimUrl(string DssApimUrl)
        {
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns(DssApimUrl);
            return this;
        }

        public EmploymentProgressionPostTriggerBuilder WithResourceFromRequest(Models.EmploymentProgression employmentProgression)
        {
            RequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(Arg.Any<HttpRequest>()).Returns(employmentProgression);
            return this;
        }

        public EmploymentProgressionPostTriggerBuilder WithEmploymentProgressionCreate(Models.EmploymentProgression employmentProgression)
        {
            if (employmentProgression == null)
            {
                EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(Arg.Any<Models.EmploymentProgression>()).Returns((Models.EmploymentProgression)null);
            }
            else
            {
                EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(Arg.Any<Models.EmploymentProgression>()).Returns(employmentProgression);
            }

            return this;
        }

        public EmploymentProgressionPostTriggerBuilder WithEmploymentProgressionExistForCustomer(bool employmentProgressionExistForCustomer)
        {
            EmploymentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(employmentProgressionExistForCustomer);
            return this;
        }

        public EmploymentProgressionPostTriggerBuilder WithCustomerReadOnly(bool customerReadOnly)
        {
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(customerReadOnly);
            return this;
        }

        public EmploymentProgressionPostTriggerBuilder WithCustomerExist(bool customerExist)
        {
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(customerExist);
            return this;
        }

        public EmploymentProgressionPostTriggerBuilder WithValidation(List<ValidationResult> validation)
        {
            Valdiator.ValidateResource(Arg.Any<Models.EmploymentProgression>()).Returns(validation);
            return this;
        }
        
        public EmploymentProgressionPostTriggerBuilder WithResourceFromRequestGenerateException()
        {
            RequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(Arg.Any<HttpRequest>()).Returns<Models.EmploymentProgression>(x => { throw new Exception(); });
            return this;
        }
    }
}
