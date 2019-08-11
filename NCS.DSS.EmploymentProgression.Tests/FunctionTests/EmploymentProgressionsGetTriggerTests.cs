using System.Threading.Tasks;
using Xunit;
using System.Net;
using NCS.DSS.EmploymentProgression.Tests.FunctionTests.Builders;
using System.Collections.Generic;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionsGetTriggerTests
    {
        //[Fact]
        //public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        //{
        //    // arrange
        //    var ResponseMessageHelper = new HttpResponseMessageHelper();
        //    var RequestHelper = Substitute.For<IHttpRequestHelper>();
        //    var EmploymentProgressionsGetTriggerService = Substitute.For<IEmploymentProgressionsGetTriggerService>();
        //    var JsonHelper = new JsonHelper();
        //    var ResourceHelper = Substitute.For<IResourceHelper>();
        //    var LoggerHelper = Substitute.For<ILoggerHelper>();

        //    var httpPostFunction = new EmploymentProgressionsGetTrigger(
        //        ResponseMessageHelper,
        //        RequestHelper,
        //        EmploymentProgressionsGetTriggerService,
        //        JsonHelper,
        //        ResourceHelper,
        //        LoggerHelper
        //        );

        //    // Act
        //    var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "");

        //    //Assert
        //    Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        //}

        //[Fact]
        //public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        //{
        //    // arrange
        //    var ResponseMessageHelper = new HttpResponseMessageHelper();
        //    var RequestHelper = Substitute.For<IHttpRequestHelper>();
        //    RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");

        //    var EmploymentProgressionsGetTriggerService = Substitute.For<IEmploymentProgressionsGetTriggerService>();
        //    var JsonHelper = new JsonHelper();
        //    var ResourceHelper = Substitute.For<IResourceHelper>();
        //    var LoggerHelper = Substitute.For<ILoggerHelper>();

        //    var httpPostFunction = new EmploymentProgressionsGetTrigger(
        //        ResponseMessageHelper,
        //        RequestHelper,
        //        EmploymentProgressionsGetTriggerService,
        //        JsonHelper,
        //        ResourceHelper,
        //        LoggerHelper
        //        );

        //    // Act
        //    var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "");

        //    //Assert
        //    Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        //}

        //[Fact]
        //public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        //{
        //    // arrange
        //    var ResponseMessageHelper = new HttpResponseMessageHelper();
        //    var RequestHelper = Substitute.For<IHttpRequestHelper>();
        //    RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
        //    RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

        //    var EmploymentProgressionsGetTriggerService = Substitute.For<IEmploymentProgressionsGetTriggerService>();
        //    var JsonHelper = new JsonHelper();
        //    var ResourceHelper = Substitute.For<IResourceHelper>();
        //    var LoggerHelper = Substitute.For<ILoggerHelper>();

        //    var httpPostFunction = new EmploymentProgressionsGetTrigger(
        //        ResponseMessageHelper,
        //        RequestHelper,
        //        EmploymentProgressionsGetTriggerService,
        //        JsonHelper,
        //        ResourceHelper,
        //        LoggerHelper
        //        );

        //    // Act
        //    var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "InvalidCustomerId");

        //    //Assert
        //    Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        //}

        //[Fact]
        //public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        //{
        //    // arrange
        //    var ResponseMessageHelper = new HttpResponseMessageHelper();
        //    var RequestHelper = Substitute.For<IHttpRequestHelper>();
        //    RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
        //    RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

        //    var EmploymentProgressionsGetTriggerService = Substitute.For<IEmploymentProgressionsGetTriggerService>();
        //    var JsonHelper = new JsonHelper();

        //    var ResourceHelper = Substitute.For<IResourceHelper>();
        //    ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

        //    var LoggerHelper = Substitute.For<ILoggerHelper>();

        //    var httpPostFunction = new EmploymentProgressionsGetTrigger(
        //        ResponseMessageHelper,
        //        RequestHelper,
        //        EmploymentProgressionsGetTriggerService,
        //        JsonHelper,
        //        ResourceHelper,
        //        LoggerHelper
        //        );

        //    // Act
        //    var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

        //    //Assert
        //    Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        //}

        //[Fact]
        //public async Task Get_RequestContainsNoErrors_ReturnOk()
        //{
        //    // arrange
        //    var ResponseMessageHelper = new HttpResponseMessageHelper();
        //    var RequestHelper = Substitute.For<IHttpRequestHelper>();
        //    RequestHelper.GetDssTouchpointId(Arg.Any<HttpRequest>()).Returns("0000000001");
        //    RequestHelper.GetDssApimUrl(Arg.Any<HttpRequest>()).Returns("http://aurlvalue.com");

        //    var EmploymentProgressionsGetTriggerService = Substitute.For<IEmploymentProgressionsGetTriggerService>();
        //    EmploymentProgressionsGetTriggerService.GetEmploymentProgressionsForCustomerAsync(Arg.Any<Guid>()).Returns(new List<Models.EmploymentProgression>());

        //    var JsonHelper = new JsonHelper();
        //    var ResourceHelper = Substitute.For<IResourceHelper>();
        //    ResourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

        //    var LoggerHelper = Substitute.For<ILoggerHelper>();

        //    var httpPostFunction = new EmploymentProgressionsGetTrigger(
        //        ResponseMessageHelper,
        //        RequestHelper,
        //        EmploymentProgressionsGetTriggerService,
        //        JsonHelper,
        //        ResourceHelper,
        //        LoggerHelper
        //        );

        //    // Act
        //    var response = await httpPostFunction.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), "844a6215-8413-41ba-96b0-b4cc7041ca33");

        //    //Assert
        //    Assert.True(response.StatusCode == HttpStatusCode.OK);
        //}

        private const string ValidCustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string ValidEmploymentProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string InvalidCustomerId = "InvalidCustomerId";
        private List<Models.EmploymentProgression> EmploymentProgressions = new List<Models.EmploymentProgression>();


        [Fact]
        public async Task  Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // Arrange
            var builder = new EmploymentProgressionGetTriggerBuilder();
            var employmentProgressionGetTrigger = builder
                .WithTouchPointId("")
                .Build();

            // Act
            var response = await employmentProgressionGetTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task  Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionGetTriggerBuilder();
            var employmentProgressionGetTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("")
                .Build();

            // Act
            var response = await employmentProgressionGetTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task  Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionGetTriggerBuilder();
            var employmentProgressionGetTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .Build();

            // Act
            var response = await employmentProgressionGetTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), InvalidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task  Get_InvalidBody_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionGetTriggerBuilder();
            var employmentProgressionGetTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithCustomerExist(true)
                .Build();

            // Act
            var response = await employmentProgressionGetTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task  Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionGetTriggerBuilder();
            var employmentProgressionGetTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithCustomerExist(false)
                .Build();

            // Act
            var response = await employmentProgressionGetTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Get_EmploymentProgressionISNull_ReturnNoContent()
        {
            // arrange
            var builder = new EmploymentProgressionGetTriggerBuilder();
            var employmentProgressionGetTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithCustomerExist(true)
                .WithEmploymentProgressionsForCustomer(null)
                .Build();

            // Act
            var response = await employmentProgressionGetTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task  Get_SuccessRequest_ReturnOk()
        {
            // arrange
            var builder = new EmploymentProgressionGetTriggerBuilder();
            var employmentProgressionGetTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithCustomerExist(true)
                .WithEmploymentProgressionsForCustomer(EmploymentProgressions)
                .Build();

            // Act
            var response = await employmentProgressionGetTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
