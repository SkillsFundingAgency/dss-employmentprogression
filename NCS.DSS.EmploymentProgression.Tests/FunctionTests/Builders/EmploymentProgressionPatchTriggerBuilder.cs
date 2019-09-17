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
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using DFC.JSON.Standard;
using System;
using Newtonsoft.Json;
using DFC.Common.Standard.GuidHelper;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests.Builders
{
    public class EmploymentProgressionPatchTriggerBuilder
    {
        public EmploymentProgressionPatch EmploymentProgressionPatch;
        public ILoggerHelper LoggerHelper { get; set; }
        public IGeoCodingService GeoService { get; set; }
        public JsonHelper JsonHelper { get; set; }
        public GuidHelper GuidHelper { get; set; }        
        public HttpResponseMessageHelper ResponseMessageHelper { get; set; }
        public IHttpRequestHelper RequestHelper { get; set; }
        public IEmploymentProgressionPatchService EmploymentProgressionPatchService { get; set; }
        public IEmploymentProgressionPatchTriggerService EmploymentProgressionPatchTriggerService { get; set; }
        public IResourceHelper ResourceHelper { get; set; }
        public IValidate Valdiator { get; set; }

        public EmploymentProgressionPatchTriggerBuilder()
        {
            LoggerHelper = Substitute.For<ILoggerHelper>();
            GeoService = Substitute.For<IGeoCodingService>();
            JsonHelper = new JsonHelper();
            GuidHelper = new GuidHelper();
            ResponseMessageHelper = new HttpResponseMessageHelper();

            RequestHelper = Substitute.For<IHttpRequestHelper>();
            EmploymentProgressionPatch = new EmploymentProgressionPatch();
            EmploymentProgressionPatchService = Substitute.For<IEmploymentProgressionPatchService>();
            EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            ResourceHelper = Substitute.For<IResourceHelper>();
            Valdiator = Substitute.For<IValidate>();
        }

        public EmploymentProgressionPatchTrigger Build()
        {
            return new EmploymentProgressionPatchTrigger(ResponseMessageHelper, RequestHelper, EmploymentProgressionPatchTriggerService,
                    JsonHelper, ResourceHelper, Valdiator, LoggerHelper, GeoService, GuidHelper, EmploymentProgressionPatchService);
        }

        public EmploymentProgressionPatchTriggerBuilder WithTouchPointId(string touchpointId)
        {
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns(touchpointId);
            return this;
        }

        public EmploymentProgressionPatchTriggerBuilder WithDssApimUrl(string DssApimUrl)
        {
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns(DssApimUrl);
            return this;
        }

        public EmploymentProgressionPatchTriggerBuilder WithResourceFromRequest(EmploymentProgressionPatch employmentProgressionPatch)
        {
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns(employmentProgressionPatch);
            return this;
        }

        public EmploymentProgressionPatchTriggerBuilder WithEmploymentProgressionPatch(EmploymentProgressionPatch employmentProgressionPatch)
        {
            if (employmentProgressionPatch == null)
            {
                EmploymentProgressionPatchService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns((string)null);
            }
            else
            {
                EmploymentProgressionPatchService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns(JsonConvert.SerializeObject(employmentProgressionPatch));
            }

            return this;
        }

        public EmploymentProgressionPatchTriggerBuilder WithEmploymentProgressionExistForCustomer(bool employmentProgressionExistForCustomer)
        {
            EmploymentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(employmentProgressionExistForCustomer);
            return this;
        }


        public EmploymentProgressionPatchTriggerBuilder WithUpdateCosmos(Models.EmploymentProgression employmentProgressionFromCosmos)
        {
            EmploymentProgressionPatchTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(employmentProgressionFromCosmos);
            return this;
        }
        

        public EmploymentProgressionPatchTriggerBuilder WithCustomerReadOnly(bool customerReadOnly)
        {
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(customerReadOnly);
            return this;
        }

        public EmploymentProgressionPatchTriggerBuilder WithCustomerExist(bool customerExist)
        {
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(customerExist);
            return this;
        }

        public EmploymentProgressionPatchTriggerBuilder WithValidation(List<ValidationResult> validation)
        {
            Valdiator.ValidateResource(Arg.Any<Models.EmploymentProgression>()).Returns(validation);
            return this;
        }

        public EmploymentProgressionPatchTriggerBuilder With(string touchpointId)
        {
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns(touchpointId);
            return this;
        }
    }
}
