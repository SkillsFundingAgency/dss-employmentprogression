using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Contact.Cosmos.Helper;
using NCS.DSS.EmployeeProgression.GeoCoding;
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
using System.Net.Http;
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
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IGeoCodingService> _geoService;
        private JsonHelper _jsonHelper;
        private Mock<GuidHelper> _guidHelper;
        private HttpResponseMessageHelper _responseMessageHelper;
        private Mock<IHttpRequestHelper> _requestHelper;
        private Mock<IEmploymentProgressionPatchService> _employmentProgressionPatchService;
        private Mock<IEmploymentProgressionPatchTriggerService> _employmentProgressionPatchTriggerService;
        private Mock<IResourceHelper> _resourceHelper;
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
            _loggerHelper = new Mock<ILoggerHelper>();
            _geoService = new Mock<IGeoCodingService>();
            _jsonHelper = new JsonHelper();
            _guidHelper = new Mock<GuidHelper>();
            _responseMessageHelper = new HttpResponseMessageHelper();
            _requestHelper = new Mock<IHttpRequestHelper>();
            _employmentProgressionPatchService = new Mock<IEmploymentProgressionPatchService>();
            _employmentProgressionPatchTriggerService = new Mock<IEmploymentProgressionPatchTriggerService>();
            _resourceHelper = new Mock<IResourceHelper>();
            _valdiator = new Mock<IValidate>();
            _EmploymentProgressionPatch = new EmploymentProgressionPatch();

            _function = new EmploymentProgressionPatchTrigger(
                _responseMessageHelper,
                _requestHelper.Object,
                _employmentProgressionPatchTriggerService.Object,
                _jsonHelper,
                _resourceHelper.Object,
                _valdiator.Object,
                _loggerHelper.Object,
                _geoService.Object,
                _guidHelper.Object,
                _employmentProgressionPatchService.Object);
            _request = new DefaultHttpRequest(new DefaultHttpContext());
        }


        [Test]
        public async Task Patch_WhenTouchPointHeaderIsMissing_ReturnBadRequest()
        {

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Patch_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
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
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
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
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Patch_InvalidBody_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(InvalidEmploymentProgressionPatch));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Patch_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task Patch_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Patch_EmploymentProgressionDoesNotExistForCustomer_ReturnBadRequest()
        {
            // arrange
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x=>x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(false);

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Patch_NoEmploymentProgressionPatchData_ReturnNoContent()
        {
            // arrange
            var employmentjson = JsonConvert.SerializeObject(_EmploymentProgressionPatch);
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _employmentProgressionPatchTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
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
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _employmentProgressionPatchTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(employmentjson));
            _employmentProgressionPatchService.Setup(x => x.PatchEmploymentProgressionAsync(It.IsAny<string>(), It.IsAny<EmploymentProgressionPatch>())).Returns(employmentjson);
            _employmentProgressionPatchTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));
            _function = new EmploymentProgressionPatchTrigger(
                _responseMessageHelper,
                _requestHelper.Object,
                _employmentProgressionPatchTriggerService.Object,
                _jsonHelper,
                _resourceHelper.Object,
                val,
                _loggerHelper.Object,
                _geoService.Object,
                _guidHelper.Object,
                _employmentProgressionPatchService.Object);

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);


            //Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task Patch_SuccessRequest_ReturnOk()
        {
            // arrange
            var employmentjson = JsonConvert.SerializeObject(_EmploymentProgressionPatch);
            _requestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _requestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https:\\someurl");
            _requestHelper.Setup(x => x.GetResourceFromRequest<EmploymentProgressionPatch>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(ValidEmploymentProgressionPatch));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _employmentProgressionPatchTriggerService.Setup(x => x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);
            _employmentProgressionPatchTriggerService.Setup(x => x.GetEmploymentProgressionForCustomerToPatchAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(employmentjson));
            _employmentProgressionPatchService.Setup(x => x.PatchEmploymentProgressionAsync(It.IsAny<string>(), It.IsAny<EmploymentProgressionPatch>())).Returns(employmentjson);
            _employmentProgressionPatchTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(new Models.EmploymentProgression()));

            // Act
            var response = await RunFunction(ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string employmentProgressionId)
        {
            return await _function.Run(_request, _logger.Object, customerId, employmentProgressionId).ConfigureAwait(false);
        }
    }
}
