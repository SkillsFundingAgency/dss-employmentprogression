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
using NCS.DSS.EmploymentProgression.PostEmploymentProgression.Service;
using NCS.DSS.EmploymentProgression.Validators;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.EmploymentProgression.Tests.FunctionTests
{
    public class EmploymentProgressionPostTriggerTests
    {
        private const string ValidCustomerId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string ValidEmploymentProgressionId = "844a6215-8413-41ba-96b0-b4cc7041ca33";
        private const string InvalidCustomerId = "InvalidCustomerId";
        private const string InvalidEmploymentProgressionId = "InvalidEmploymentProgressionId";

        private Models.EmploymentProgression ValidEmploymentProgression = new Models.EmploymentProgression();
        private Models.EmploymentProgression InvalidEmploymentProgression = null;
        private List<ValidationResult> ValidationResultNoErrors = new List<ValidationResult>();

        private List<ValidationResult> ValidationResultOneError = new List<ValidationResult>()
        {
            new ValidationResult("Please supply a valid value for Economic Shock Status", new[] { "EconomicShockStatus" })
        };
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IGeoCodingService> _geoService;
        private JsonHelper _jsonHelper;
        private Mock<GuidHelper> _guidHelper;
        private HttpResponseMessageHelper _responseMessageHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IValidate> _valdiator;
        private HttpRequest _request;
        private EmploymentProgressionPostTrigger _function;
        private Mock<ILogger> _logger;
        private Mock<IEmploymentProgressionPostTriggerService> _employmentProgressionPostTriggerService;
        private Models.EmploymentProgression _employmentProgression;


         [SetUp]
        public void Setup()
        {
            _employmentProgression = new Models.EmploymentProgression() { CustomerId = Guid.NewGuid() };
            _logger = new Mock<ILogger>();
            _loggerHelper = new Mock<ILoggerHelper>();
            _geoService = new Mock<IGeoCodingService>();
            _jsonHelper = new JsonHelper();
            _guidHelper = new Mock<GuidHelper>();
            _responseMessageHelper = new HttpResponseMessageHelper();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _employmentProgressionPostTriggerService = new Mock<IEmploymentProgressionPostTriggerService>();
            _resourceHelper = new Mock<IResourceHelper>();
            _valdiator = new Mock<IValidate>();

            _function = new EmploymentProgressionPostTrigger(
                _responseMessageHelper,
                _httpRequestHelper.Object,
                _employmentProgressionPostTriggerService.Object,
                _jsonHelper,
                _resourceHelper.Object,
                _valdiator.Object,
                _loggerHelper.Object,
                _geoService.Object,
                _guidHelper.Object);
            _request = new DefaultHttpRequest(new DefaultHttpContext());
        }

        [Test]
        public async Task Post_WhenTouchPointHeaderIsMissing_ReturnBadRequest()
        {

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Post_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
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
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Post_InvalidBody_ReturnUnprocessableEntity()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Throws(new Exception());
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task Post_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.IsCustomerReadOnly(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Test]
        public async Task Post_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression)); 

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Test]
        public async Task Post_EmploymentProgressionExistForCustomer_ReturnConflict()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));
            _employmentProgressionPostTriggerService.Setup(x=>x.DoesEmploymentProgressionExistForCustomer(It.IsAny<Guid>())).Returns(true);

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Test]
        public async Task Post_GetFromResourceIsNull_ReturnUnprocessableEntityt()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(default(Models.EmploymentProgression)));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task Post_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));
            var val = new Validate();

            _function = new EmploymentProgressionPostTrigger(
                _responseMessageHelper,
                _httpRequestHelper.Object,
                _employmentProgressionPostTriggerService.Object,
                _jsonHelper,
                _resourceHelper.Object,
                val,
                _loggerHelper.Object,
                _geoService.Object,
                _guidHelper.Object);

            // Act
            var response = await RunFunction(ValidCustomerId);


            //Assert
            Assert.AreEqual(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        }

        [Test]
        public async Task Post_SuccessfulRequest_ReturnCreated()
        {
            // arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(It.IsAny<HttpRequest>())).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(It.IsAny<HttpRequest>())).Returns("https://someurl.com");
            _httpRequestHelper.Setup(x=>x.GetResourceFromRequest<Models.EmploymentProgression>(It.IsAny<HttpRequest>())).Returns(Task.FromResult(_employmentProgression));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _valdiator.Setup(x => x.ValidateResource(It.IsAny<Models.EmploymentProgression>())).Returns(new List<ValidationResult>());
            _employmentProgressionPostTriggerService.Setup(x => x.CreateEmploymentProgressionAsync(It.IsAny<Models.EmploymentProgression>())).Returns(Task.FromResult(_employmentProgression));

            // Act
            var response = await RunFunction(ValidCustomerId);

            //Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await _function.Run(_request, _logger.Object, customerId).ConfigureAwait(false);
        }
    }
}
