using System.Threading.Tasks;
using Xunit;
using System.Net;
using NSubstitute;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using NCS.DSS.Contact.Cosmos.Helper;
using DFC.Common.Standard.Logging;
using Microsoft.AspNetCore.Http;
using System;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using DFC.Common.Standard.GuidHelper;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionGetByIdTriggerTests
    {
        [Fact]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();

            var EmploymentProgressionGetByIdService = Substitute.For<IEmploymentProgressionGetByIdTriggerService>();
            EmploymentProgressionGetByIdService.GetEmploymentProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var GuidHelper = new GuidHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionGetByIdTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                LoggerHelper,
                GuidHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "", "");

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

            var EmploymentProgressionGetByIdService = Substitute.For<IEmploymentProgressionGetByIdTriggerService>();
            EmploymentProgressionGetByIdService.GetEmploymentProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var GuidHelper = new GuidHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();


            var httpPostFunction = new EmploymentProgressionGetByIdTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                LoggerHelper,
                GuidHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "", "");

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

            var EmploymentProgressionGetByIdService = Substitute.For<IEmploymentProgressionGetByIdTriggerService>();
            EmploymentProgressionGetByIdService.GetEmploymentProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var GuidHelper = new GuidHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            var LoggerHelper = Substitute.For<ILoggerHelper>();


            var httpPostFunction = new EmploymentProgressionGetByIdTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                LoggerHelper,
                GuidHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "InvalidCustomerId", "");

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

            var EmploymentProgressionGetByIdService = Substitute.For<IEmploymentProgressionGetByIdTriggerService>();
            EmploymentProgressionGetByIdService.GetEmploymentProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var GuidHelper = new GuidHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionGetByIdTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                LoggerHelper,
                GuidHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33", "");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_RequestContainsNoErrors_ReturnOk()
        {
            // arrange
            var ResponseMessageHelper = new HttpResponseMessageHelper();
            var RequestHelper = Substitute.For<IHttpRequestHelper>();
            RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
            RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

            var EmploymentProgressionGetByIdService = Substitute.For<IEmploymentProgressionGetByIdTriggerService>();
            EmploymentProgressionGetByIdService.GetEmploymentProgressionForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(new Models.EmploymentProgression());

            var JsonHelper = new JsonHelper();
            var GuidHelper = new GuidHelper();
            var ResourceHelper = Substitute.For<IResourceHelper>();
            ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var LoggerHelper = Substitute.For<ILoggerHelper>();

            var httpPostFunction = new EmploymentProgressionGetByIdTrigger(
                ResponseMessageHelper,
                RequestHelper,
                EmploymentProgressionGetByIdService,
                JsonHelper,
                ResourceHelper,
                LoggerHelper,
                GuidHelper
                );

            // Act
            var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33", "844a6215-8413-41ba-96b0-b4cc7041ca33");

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
