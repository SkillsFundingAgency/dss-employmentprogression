using System.Threading.Tasks;
using Xunit;
using System.Net;
using NSubstitute;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using DFC.Common.Standard.Logging;
using Microsoft.AspNetCore.Http;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Validators;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionPostTriggerTests
    {
        [Fact]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "");

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

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "");

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

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "InvalidCustomerId");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_RequestContainsNoErrors_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(true);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_EmploymentProgressionAlreadyExist_ReturnConflict()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());
            EmploymentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(true);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Get_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());
            EmploymentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(false);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();

            var validationResult = new ValidationResult("Error MEssage");
            ErrorResults.Add(validationResult);
            Valdiator.ValidateResource(Arg.Any<Models.EmploymentProgression>()).Returns(ErrorResults);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Get_UnableToCreateEmploymentProgression_ReturnBadRequest()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            Models.EmploymentProgression employmentProgression = new Models.EmploymentProgression();
            RequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(Arg.Any<HttpRequest>()).Returns(employmentProgression);

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(new Models.EmploymentProgression()).Returns(new Models.EmploymentProgression());
            EmploymentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(false);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();
            Valdiator.ValidateResource(Arg.Any<Models.EmploymentProgression>()).Returns(ErrorResults);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_SuccessRequest_ReturnOk()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");
            var employmentProgression = new Models.EmploymentProgression();
            RequestHelper.GetResourceFromRequest<Models.EmploymentProgression>(Arg.Any<HttpRequest>()).Returns(employmentProgression);

            var EmploymentProgressionPostTriggerService = Substitute.For<IEmploymentProgressionPostTriggerService>();
            EmploymentProgressionPostTriggerService.CreateEmploymentProgressionAsync(Arg.Any<Models.EmploymentProgression>()).Returns(employmentProgression);
            EmploymentProgressionPostTriggerService.DoesEmploymentProgressionExistForCustomer(Arg.Any<Guid>()).Returns(false);

            var JsonHelper = new JsonHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.IsCustomerReadOnly(Arg.Any<Guid>()).Returns(false);
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var Valdiator = Substitute.For<IValidate>();
            List<ValidationResult> ErrorResults = new List<ValidationResult>();
            Valdiator.ValidateResource(Arg.Any<Models.EmploymentProgression>()).Returns(ErrorResults);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionPostTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionPostTriggerService,
                JsonHelper,
                ResourceHelper,
                Valdiator,
                LoggerHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
