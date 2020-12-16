using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionsGetTriggerTests
    {
        private const string ValidCustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string ValidEmploymentProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string InvalidCustomerId = "InvalidCustomerId";
        private List<Models.EmploymentProgression> EmploymentProgressions = new List<Models.EmploymentProgression>();
        private EmploymentProgressionGetTrigger _function;
        private IHttpResponseMessageHelper _httpResponseHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IEmploymentProgressionGetTriggerService> _employmentTriggerService;
        private JsonHelper _jsonHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IGuidHelper> _guidHelper;
        private HttpRequest _request;
        private Mock<ILogger> _logger;

        [SetUp]
        public void Setup()
        {
            _httpResponseHelper = new HttpResponseMessageHelper();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _employmentTriggerService = new Mock<IEmploymentProgressionGetTriggerService>();
            _jsonHelper = new JsonHelper(); ;
            _resourceHelper = new Mock<IResourceHelper>();
            _loggerHelper = new Mock<ILoggerHelper>(); ;
            _guidHelper = new Mock<IGuidHelper>();
            _logger = new Mock<ILogger>();
            _function = new EmploymentProgressionGetTrigger(_httpResponseHelper, _httpRequestHelper.Object, _employmentTriggerService.Object, _jsonHelper, _resourceHelper.Object, _loggerHelper.Object, _guidHelper.Object);
            _request = new DefaultHttpRequest(new DefaultHttpContext());

        }

        [Test]
        public async Task Get_WhenTouchPointHeaderIsMissing_ReturnBadRequest()
        {
            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual( HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");

            // Act
            var response = await RunFunction(InvalidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Get_EmploymentProgressionISNull_ReturnNoContent()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Get_SuccessRequest_ReturnOk()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentTriggerService.Setup(x => x.GetEmploymentProgressionsForCustomerAsync(It.IsAny<Guid>())).Returns(Task.FromResult(new List<Models.EmploymentProgression>()));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await _function.Run(_request, _logger.Object, customerId).ConfigureAwait(false);
        }
    }

}