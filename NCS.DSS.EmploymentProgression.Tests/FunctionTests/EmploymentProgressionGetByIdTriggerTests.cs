using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
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
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<ILogger<EmploymentProgressionGetByIdTrigger>> _loggerHelper;
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
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _loggerHelper = new Mock<ILogger<EmploymentProgressionGetByIdTrigger>>(); 
            _logger = new Mock<ILogger>();
            _function = new EmploymentProgressionGetByIdTrigger(_httpRequestHelper.Object, _EmploymentProgressionGetByIdTriggerService.Object, _convertToDynamic.Object, _cosmosDbProvider.Object, _loggerHelper.Object);
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
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction("844a6215-8413-41ba-96b0-b4cc7041ca33", "");

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_RequestContainsNoErrors_ReturnOk()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("http://aurlvalue.com");
            _EmploymentProgressionGetByIdTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

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
