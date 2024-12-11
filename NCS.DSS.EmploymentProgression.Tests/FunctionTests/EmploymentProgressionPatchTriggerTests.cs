using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.EmployeeProgression.GeoCoding;
using NCS.DSS.EmploymentProgression.Cosmos.Provider;
using NCS.DSS.EmploymentProgression.Function;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Validators;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionPatchTriggerTests
    {
        private const string ValidCustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string ValidEmploymentProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string InvalidCustomerId = "InvalidCustomerId";
        private const string InvalidEmploymentProgressionId = "InvalidEmploymentProgressionId";
        private EmploymentProgressionPatch ValidEmploymentProgressionPatch = new EmploymentProgressionPatch();
        private EmploymentProgressionPatch InvalidEmploymentProgressionPatch = null;
        private List<ValidationResult> ValidationResultNoErrors = new List<ValidationResult>();
        private EmploymentProgressionPatch _EmploymentProgressionPatch;
        private Mock<ILogger<EmploymentProgressionPatchTrigger>> _loggerHelper;
        private Mock<IGeoCodingService> _geoService;
        private Mock<IConvertToDynamic<Models.EmploymentProgression>> _convertToDynamic;
        private Mock<IHttpRequestHelper> _requestHelper;
        private Mock<IEmploymentProgressionPatchService> _employmentProgressionPatchService;
        private Mock<IEmploymentProgressionPatchTriggerService> _employmentProgressionPatchTriggerService;
        private Mock<ICosmosDBProvider> _cosmosDbProvider;
        private Mock<IValidate> _valdiator;
        private HttpRequest _request;
        private EmploymentProgressionPatchTrigger _function;
        private Mock<ILogger> _logger;

        private List<ValidationResult> ValidationResultOneError = new List<ValidationResult>()
        {
            new ValidationResult("Please supply a valid value for Economic Shock Status", new[] { "EconomicShockStatus" })
        };


        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger>();
            _loggerHelper = new Mock<ILogger<EmploymentProgressionPatchTrigger>>();
            _geoService = new Mock<IGeoCodingService>();
            _convertToDynamic = new Mock<IConvertToDynamic<Models.EmploymentProgression>>();
            _requestHelper = new Mock<IHttpRequestHelper>();
            _employmentProgressionPatchService = new Mock<IEmploymentProgressionPatchService>();
            _employmentProgressionPatchTriggerService = new Mock<IEmploymentProgressionPatchTriggerService>();
            _cosmosDbProvider = new Mock<ICosmosDBProvider>();
            _valdiator = new Mock<IValidate>();
            _EmploymentProgressionPatch = new EmploymentProgressionPatch();

            _function = new EmploymentProgressionPatchTrigger(
                _requestHelper.Object,
                _employmentProgressionPatchTriggerService.Object,
                _convertToDynamic.Object,
                _cosmosDbProvider.Object,
                _valdiator.Object,
                _loggerHelper.Object,
                _geoService.Object,
                _employmentProgressionPatchService.Object);
            _request = (new DefaultHttpContext()).Request;
        }


        [Test]
        public async Task Patch_WhenTouchPointHeaderIsMissing_ReturnBadRequest()
        {

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Patch_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }



        [Test]
        public async Task Patch_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");

            // Act
            var response = await RunFunction(InvalidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Patch_EmploymentProgressionIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");

            // Act
            var response = await RunFunction(ValidCustomerId, InvalidEmploymentProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Patch_InvalidBody_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(InvalidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerHaveATerminationDate(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Patch_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerHaveATerminationDate(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);
            var objResponse = (ObjectResult)response;
            //Assert
            Assert.That(response, Is.InstanceOf<ObjectResult>());
            Assert.That(objResponse.StatusCode, Is.EqualTo((int)HttpStatusCode.Forbidden));
        }

        [Test]
        public async Task Patch_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerHaveATerminationDate(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<BadRequestResult>());
        }

        [Test]
        public async Task Patch_EmploymentProgressionDoesNotExistForCustomer_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerHaveATerminationDate(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Patch_NoEmploymentProgressionPatchData_ReturnNoContent()
        {
            // arrange
            var employmentjson = JsonConvert.SerializeObject(_EmploymentProgressionPatch);
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerHaveATerminationDate(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.That(response, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task Patch_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            var val = new Validate();
            var employmentjson = JsonConvert.SerializeObject(_EmploymentProgressionPatch);
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerHaveATerminationDate(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(employmentjson));
            _employmentProgressionPatchService.Setup(x => x.PatchEmploymentProgressionAsync(It.IsAny<string>(), It.IsAny<EmploymentProgressionPatch>())).Returns(employmentjson);
            _employmentProgressionPatchTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));
            _function = new EmploymentProgressionPatchTrigger(
                _requestHelper.Object,
                _employmentProgressionPatchTriggerService.Object,
                _convertToDynamic.Object,
                _cosmosDbProvider.Object,
                val,
                _loggerHelper.Object,
                _geoService.Object,
                _employmentProgressionPatchService.Object);

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);


            //Assert
            Assert.That(response, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task Patch_SuccessRequest_ReturnOk()
        {
            // arrange
            var employmentjson = JsonConvert.SerializeObject(_EmploymentProgressionPatch);
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _cosmosDbProvider.Setup(x => x.DoesCustomerHaveATerminationDate(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _cosmosDbProvider.Setup(x => x.DoesCustomerResourceExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(employmentjson));
            _employmentProgressionPatchService.Setup(x => x.PatchEmploymentProgressionAsync(It.IsAny<string>(), It.IsAny<EmploymentProgressionPatch>())).Returns(employmentjson);
            _employmentProgressionPatchTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);
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
