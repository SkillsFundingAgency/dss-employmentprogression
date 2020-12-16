using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionGetByIdTriggerTests
    {
        private EmploymentProgressionGetByIdTrigger _function;
        private IHttpResponseMessageHelper _httpResponseHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IEmploymentProgressionGetByIdTriggerService> _EmploymentProgressionGetByIdTriggerService;
        private IJsonHelper _jsonHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IGuidHelper> _guidHelper;
        private Mock<ILogger> _logger;
        private HttpRequest _request;
        private Guid _validCustomerId = Guid.NewGuid();
        private Guid _validEmploymentProgressionId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _httpResponseHelper = new HttpResponseMessageHelper();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _EmploymentProgressionGetByIdTriggerService = new Mock<IEmploymentProgressionGetByIdTriggerService>();
            _jsonHelper = new JsonHelper(); ;
            _resourceHelper = new Mock<IResourceHelper>();
            _loggerHelper = new Mock<ILoggerHelper>(); ;
            _guidHelper = new Mock<IGuidHelper>();
            _logger = new Mock<ILogger>();
            _function = new EmploymentProgressionGetByIdTrigger(_httpResponseHelper, _httpRequestHelper.Object, _EmploymentProgressionGetByIdTriggerService.Object, _jsonHelper, _resourceHelper.Object, _loggerHelper.Object, _guidHelper.Object);
            _request = new DefaultHttpRequest(new DefaultHttpContext());
            
        }


        [Test]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // Arrange
            _EmploymentProgressionGetByIdTriggerService.Setup(x=>x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));

            // Act
            var response = await RunFunction("", "");

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _EmploymentProgressionGetByIdTriggerService.Setup(x=>x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));

            // Act
            var response = await RunFunction("", "");

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x=>x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _EmploymentProgressionGetByIdTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));

            // Act
            var response = await RunFunction("InvalidCustomerId", "");

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode );
        }

        [Test]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _EmploymentProgressionGetByIdTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction("844a6215-8413-41ba-96b0-b4cc7041ca33", "");

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Get_RequestContainsNoErrors_ReturnOk()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _EmploymentProgressionGetByIdTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _guidHelper.Setup(x => x.IsValidGuid(It.IsAny<string>())).Returns(true);

            // Act
            var response = await RunFunction(_validCustomerId.ToString(), _validEmploymentProgressionId.ToString());

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string employmentProgressionId)
        {
            return await _function.Run(_request, _logger.Object,customerId, employmentProgressionId).ConfigureAwait(false);
        }
    }
}
