using DFC.Common.Standard.GuidHelper;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmploymentProgression.GetEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
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
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IEmploymentProgressionGetTriggerService> _employmentTriggerService;
        private IConvertToDynamic<Models.EmploymentProgression> _convertToDynamic;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<ILogger<EmploymentProgressionGetTrigger>> _loggerHelper;
        private Mock<IGuidHelper> _guidHelper;
        private HttpRequest _request;

        [SetUp]
        public void Setup()
        {
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _employmentTriggerService = new Mock<IEmploymentProgressionGetTriggerService>();
            _convertToDynamic = new ConvertToDynamic<Models.EmploymentProgression>(); ;
            _resourceHelper = new Mock<IResourceHelper>();
            _loggerHelper = new Mock<ILogger<EmploymentProgressionGetTrigger>>(); ;
            _guidHelper = new Mock<IGuidHelper>();
            _function = new EmploymentProgressionGetTrigger(_httpRequestHelper.Object, _employmentTriggerService.Object, _convertToDynamic, _resourceHelper.Object, _loggerHelper.Object, _guidHelper.Object);
            _request = (new DefaultHttpContext()).Request;
        }

        [Test]
        public async Task Get_WhenTouchPointHeaderIsMissing_ReturnBadRequest()
        {
            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Get_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
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
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
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
            Assert.That(response, Is.InstanceOf<NoContentResult>());
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
            Assert.That(response, Is.InstanceOf<NoContentResult>());
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
            var jsonResponse = (JsonResult)response;
            //Assert
            Assert.That(response, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(_request, customerId).ConfigureAwait(false);
        }
    }
}