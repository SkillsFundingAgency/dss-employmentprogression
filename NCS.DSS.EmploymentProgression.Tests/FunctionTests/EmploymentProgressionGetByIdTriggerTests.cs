using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.GetEmploymentProgressionById.Service;
using NCS.DSS.EmploymentProgression.Models;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionGetByIdTriggerTests
    {
        private EmploymentProgressionGetByIdTrigger _function;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IConvertToDynamic<Models.EmploymentProgression>> _convertToDynamic;
        private Mock<IEmploymentProgressionGetByIdTriggerService> _EmploymentProgressionGetByIdTriggerService;
        private IJsonHelper _jsonHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILogger<EmploymentProgressionGetByIdTrigger>> _loggerHelper;
        private Mock<IGuidHelper> _guidHelper;
        private Mock<ILogger> _logger;
        private HttpRequest _request;
        private Guid _validCustomerId = Guid.NewGuid();
        private Guid _validEmploymentProgressionId = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _convertToDynamic = new Mock<IConvertToDynamic<Models.EmploymentProgression>>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _EmploymentProgressionGetByIdTriggerService = new Mock<IEmploymentProgressionGetByIdTriggerService>();
            _jsonHelper = new JsonHelper(); ;
            _resourceHelper = new Mock<IResourceHelper>();
            _loggerHelper = new Mock<ILogger<EmploymentProgressionGetByIdTrigger>>(); ;
            _guidHelper = new Mock<IGuidHelper>();
            _logger = new Mock<ILogger>();
            _function = new EmploymentProgressionGetByIdTrigger(_httpRequestHelper.Object, _EmploymentProgressionGetByIdTriggerService.Object, _convertToDynamic.Object, _resourceHelper.Object, _loggerHelper.Object, _guidHelper.Object);
            _request = (new DefaultHttpContext()).Request;
        }


        [Test]
        public async Task Get_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // Arrange
            _EmploymentProgressionGetByIdTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));

            // Act
            var response = await RunFunction("", "");

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _EmploymentProgressionGetByIdTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));

            // Act
            var response = await RunFunction("", "");

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _EmploymentProgressionGetByIdTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));

            // Act
            var response = await RunFunction("InvalidCustomerId", "");

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Get_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _EmploymentProgressionGetByIdTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction("844a6215-8413-41ba-96b0-b4cc7041ca33", "");

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
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
            var jsonResponse = (JsonResult)response;

            //Assert
            Assert.That(response, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string employmentProgressionId)
        {
            return await _function.Run(_request, customerId, employmentProgressionId).ConfigureAwait(false);
        }
    }
}
