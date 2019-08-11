using System.Threading.Tasks;
using Xunit;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using NCS.DSS.EmploymentProgression.Tests.FunctionTests.Builders;

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

        [Fact]
        public async Task Post_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // Arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("")
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("")
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), InvalidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_InvalidBody_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(InvalidEmploymentProgression)
                .WithEmploymentProgressionCreate(InvalidEmploymentProgression)
                .WithCustomerExist(true)
                .WithCustomerReadOnly(false)
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Post_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgression)
                .WithEmploymentProgressionCreate(InvalidEmploymentProgression)
                .WithEmploymentProgressionExistForCustomer(true)
                .WithCustomerReadOnly(true)
                .WithCustomerExist(true)
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Post_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgression)
                .WithEmploymentProgressionCreate(InvalidEmploymentProgression)
                .WithEmploymentProgressionExistForCustomer(false)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(false)
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Post_EmploymentProgressionDoesNotExistForCustomer_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgression)
                .WithEmploymentProgressionCreate(InvalidEmploymentProgression)
                .WithEmploymentProgressionExistForCustomer(false)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(false)
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Post_EmploymentProgressionExistForCustomer_ReturnConflict()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgression)
                .WithEmploymentProgressionCreate(InvalidEmploymentProgression)
                .WithEmploymentProgressionExistForCustomer(true)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(true)
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Post_GetFromResourceIsNull_ReturnNoContent()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgression)
                .WithEmploymentProgressionCreate(InvalidEmploymentProgression)
                .WithEmploymentProgressionExistForCustomer(false)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(true)
                .WithResourceFromRequest(null)
                .WithValidation(ValidationResultOneError)
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Post_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgression)
                .WithEmploymentProgressionCreate(InvalidEmploymentProgression)
                .WithEmploymentProgressionExistForCustomer(false)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(true)
                .WithValidation(ValidationResultOneError)
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);


            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Post_SuccessRequest_ReturnOk()
        {
            // arrange
            var builder = new EmploymentProgressionPostTriggerBuilder();
            var employmentProgressionPostTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgression)
                .WithEmploymentProgressionCreate(ValidEmploymentProgression)
                .WithEmploymentProgressionExistForCustomer(false)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(true)
                .WithValidation(ValidationResultNoErrors)
                .Build();

            // Act
            var response = await employmentProgressionPostTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
