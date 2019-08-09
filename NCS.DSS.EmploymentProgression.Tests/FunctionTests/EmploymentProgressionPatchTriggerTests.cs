using System.Threading.Tasks;
using Xunit;
using System.Net;
using NSubstitute;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using DFC.Common.Standard.Logging;
using Microsoft.AspNetCore.Http;
using NCS.DSS.EmploymentProgression.Validators;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Newtonsoft.Json;
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Function;
using NCS.DSS.EmploymentProgression.Models;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionPatchTriggerTests
    {
        const string CustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidCustomerId = "InvalidCustomerId";
        const string EmploymentProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        const string InvalidEmploymentProgressionId = "InvalidEmploymentProgressionId";

        [Fact]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), InvalidCustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_EmploymentProgressionIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, InvalidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_InvalidBody_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns((EmploymentProgressionPatch)null);

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns("AString");


            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Get_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new EmploymentProgressionPatch());

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns("AString");

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(true);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new EmploymentProgressionPatch());

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns(JsonConvert.SerializeObject(new EmploymentProgressionPatch()));
            EmploymentProgressionPatchTriggerService.GetEmploymentProgressionForCustomerToPatchAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(JsonConvert.SerializeObject(new EmploymentProgressionPatch()));
            EmploymentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(true);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(false);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();

            var validationResult = new ValidationResult("Error MEssage");
            ErrorResults.Add(validationResult);
            Valdiator.ValidateResource(Arg.Any<Models.EmploymentProgression>()).Returns(ErrorResults);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_EmploymentProvideDoesNotExistForCustomer_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new EmploymentProgressionPatch());

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns("AString");
            EmploymentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(false);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_NoEmploymentProgressionPatchData_ReturnNoContent()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new EmploymentProgressionPatch());

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns("AString");
            EmploymentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(true);
            EmploymentProgressionPatchTriggerService.GetEmploymentProgressionForCustomerToPatchAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns((string)null);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Get_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new EmploymentProgressionPatch());

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns(JsonConvert.SerializeObject(new EmploymentProgressionPatch()));
            EmploymentProgressionPatchTriggerService.GetEmploymentProgressionForCustomerToPatchAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(JsonConvert.SerializeObject(new EmploymentProgressionPatch()));
            EmploymentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(true);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();

            var validationResult = new ValidationResult("Error MEssage");
            ErrorResults.Add(validationResult);
            Valdiator.ValidateResource(Arg.Any<Models.EmploymentProgression>()).Returns(ErrorResults);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Get_SuccessRequest_ReturnOk()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns(new EmploymentProgressionPatch());

            var employmentProgressionPatch = new EmploymentProgressionPatch();
            RequestHelper.GetResourceFromRequest<EmploymentProgressionPatch>(Arg.Any<HttpRequest>()).Returns(employmentProgressionPatch);

            var EmploymentProgressionPatchTriggerService = Substitute.For<IEmploymentProgressionPatchTriggerService>();
            EmploymentProgressionPatchTriggerService.PatchEmploymentProgressionAsync(Arg.Any<string>(), Arg.Any<EmploymentProgressionPatch>()).Returns(JsonConvert.SerializeObject(new EmploymentProgressionPatch()));
            EmploymentProgressionPatchTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(true);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();
            Valdiator.ValidateResource(Arg.Any<Models.EmploymentProgression>()).Returns(ErrorResults);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPatchTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPatchTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), CustomerId, EmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
