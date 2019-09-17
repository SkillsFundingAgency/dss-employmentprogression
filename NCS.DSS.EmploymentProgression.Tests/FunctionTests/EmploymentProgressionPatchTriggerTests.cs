using System.Threading.Tasks;
using Xunit;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using NCS.DSS.EmploymentProgression.Models;
using NCS.DSS.EmploymentProgression.Tests.FunctionTests.Builders;

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
        private List<ValidationResult> ValidationResultOneError = new List<ValidationResult>()
        {
            new ValidationResult("Please supply a valid value for Economic Shock Status", new[] { "EconomicShockStatus" })
        };

        [Fact]
        public async Task Patch_WhenTouchPointHeaderIsMission_ReturnBadRequest()
        {
            // Arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("")
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Patch_WhenGetDssApimUrlGetDssApimUrlIsEMpty_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("")
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Patch_CustomerIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), InvalidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Patch_EmploymentProgressionIdIsNotValidGuid_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, InvalidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Patch_InvalidBody_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(InvalidEmploymentProgressionPatch)
                .WithEmploymentProgressionPatch(InvalidEmploymentProgressionPatch)
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Patch_ReadOnlyCustomer_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionPatch(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionExistForCustomer(true)
                .WithCustomerReadOnly(true)
                .WithCustomerExist(true)
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Patch_CustomerIdIsValidGuidButCustomerDoesNotExist_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionPatch(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionExistForCustomer(false)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(false)
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Patch_EmploymentProgressionDoesNotExistForCustomer_ReturnBadRequest()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionPatch(InvalidEmploymentProgressionPatch)
                .WithEmploymentProgressionExistForCustomer(false)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(true)
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Patch_NoEmploymentProgressionPatchData_ReturnNoContent()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionPatch(InvalidEmploymentProgressionPatch)
                .WithEmploymentProgressionExistForCustomer(true)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(true)
                .WithUpdateCosmos(null)
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Patch_ValidationFailed_ReturnUnprocessableEntity()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionPatch(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionExistForCustomer(true)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(true)
                .WithValidation(ValidationResultOneError)
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);


            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.UnprocessableEntity);
        }

        [Fact]
        public async Task Patch_SuccessRequest_ReturnOk()
        {
            // arrange
            var builder = new EmploymentProgressionPatchTriggerBuilder();
            var employmentProgressionPatchTrigger = builder
                .WithTouchPointId("0000000001")
                .WithDssApimUrl("http://www.google.com")
                .WithResourceFromRequest(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionPatch(ValidEmploymentProgressionPatch)
                .WithEmploymentProgressionExistForCustomer(true)
                .WithCustomerReadOnly(false)
                .WithCustomerExist(true)
                .WithUpdateCosmos(new Models.EmploymentProgression())
                .WithValidation(ValidationResultNoErrors)
                .Build();

            // Act
            var response = await employmentProgressionPatchTrigger.Run(TestFactory.CreateHttpRequest("", ""), TestFactory.CreateLogger(), ValidCustomerId, ValidEmploymentProgressionId);

            //Assert
            Assert.True(response.StatusCode == HttpStatusCode.OK);
        }
    }
}
