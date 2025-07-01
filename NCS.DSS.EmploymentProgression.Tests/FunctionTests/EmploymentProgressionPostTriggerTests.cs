using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Validators;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionPostTriggerTests
    {
        private const string ValidCustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string ValidEmploymentProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string InvalidCustomerId = "InvalidCustomerId";
        private const string InvalidEmploymentProgressionId = "InvalidEmploymentProgressionId";

        private List<ValidationResult> ValidationResultNoErrors = new List<ValidationResult>();

        private List<ValidationResult> ValidationResultOneError = new List<ValidationResult>()
        {
            new ValidationResult("Please supply a valid value for Economic Shock Status", new[] { "EconomicShockStatus" })
        };
        private Mock<ILogger<EmploymentProgressionPostTrigger>> _loggerHelper;
        private Mock<IConvertToDynamic<Models.EmploymentProgression>> _convertToDynamic;
        private Mock<IGeoCodingService> _geoService;
        private JsonHelper _jsonHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<IValidate> _valdiator;
        private HttpRequest _request;
        private EmploymentProgressionPostTrigger _function;
        private Mock<IEmploymentProgressionPostTriggerService> _employmentProgressionPostTriggerService;
        private Models.EmploymentProgression _employmentProgression;


        [SetUp]
        public void Setup()
        {
            _employmentProgression = new Models.EmploymentProgression() { CustomerId = Guid.NewGuid() };
            _convertToDynamic = new Mock<IConvertToDynamic<Models.EmploymentProgression>>();
            _loggerHelper = new Mock<ILogger<EmploymentProgressionPostTrigger>>();
            _geoService = new Mock<IGeoCodingService>();
            _jsonHelper = new JsonHelper();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _employmentProgressionPostTriggerService = new Mock<IEmploymentProgressionPostTriggerService>();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _valdiator = new Mock<IValidate>();

            _function = new EmploymentProgressionPostTrigger(
                _httpRequestHelper.Object,
                _employmentProgressionPostTriggerService.Object,
                _cosmosDbProvider.Object,
                _valdiator.Object,
                _loggerHelper.Object,
                _geoService.Object,
                _convertToDynamic.Object);
            _request = (new DefaultHttpContext()).Request;
        }

        [Test]
        public async Task Post_WhenTouchPointHeaderIsMissing_ReturnBadRequest()
        {

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Post_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Post_CustomerIdIsNotValidGuid_ReturnBadRequest()
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
        public async Task Post_InvalidBody_ReturnUnprocessableEntity()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Throws(new Exception());
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task Post_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerHaveATerminationDate(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));

            // Act
            var response = await RunFunction(ValidCustomerId);
            var objResponse = (ObjectResult)response;
            //Assert
            Assert.That(response, Is.InstanceOf<ObjectResult>());
            Assert.That(objResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task Post_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task Post_EmploymentProgressionExistForCustomer_ReturnConflict()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));
            _employmentProgressionPostTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<ConflictObjectResult>());
        }

        [Test]
        public async Task Post_GetFromResourceIsNull_ReturnUnprocessableEntityt()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(default(Models.EmploymentProgression)));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.That(response, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task Post_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));
            var val = new Validate();

            _function = new EmploymentProgressionPostTrigger(
                _httpRequestHelper.Object,
                _employmentProgressionPostTriggerService.Object,
                _cosmosDbProvider.Object,
                val,
                _loggerHelper.Object,
                _geoService.Object,
                _convertToDynamic.Object);

            // Act
            var response = await RunFunction(ValidCustomerId);


            //Assert
            Assert.That(response, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task Post_SuccessfulRequest_SetDefaultsCalled()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));

            // Act
            _ = await RunFunction(ValidCustomerId);

            //Assert
            _employmentProgressionPostTriggerService.Verify(x => x.SetDefaults(It.IsAny<Models.EmploymentProgression>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Post_SuccessfulRequest_ReturnCreated()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));

            // Act
            var response = await RunFunction(ValidCustomerId);
            var jsonResponse = (JsonResult)response;
            //Assert
            Assert.That(response, Is.InstanceOf<JsonResult>());
            Assert.That(jsonResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(_request, customerId).ConfigureAwait(false);
        }
    }
}
