﻿using NCS.DSS.EmploymentProgression.Validators;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.EmploymentProgression.Tests.ValidationTests
{
    [TestFixture]
    public class ValidationTests_Patch
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsValid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                LastModifiedTouchpointId = "0000000001"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedTouchpointIdIsInvalid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                LastModifiedTouchpointId = "000000000A"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEmployerPostcodeIsValid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                EconomicShockStatus = ReferenceData.EconomicShockStatus.NotApplicable,
                DateProgressionRecorded = DateTime.UtcNow,
                CurrentEmploymentStatus = ReferenceData.CurrentEmploymentStatus.NotKnown,
                LastModifiedTouchpointId = "0000000001",
                EmployerPostcode = "CV12 1CS"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEmployerPostcodeIsInvalid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                EconomicShockStatus = ReferenceData.EconomicShockStatus.NotApplicable,
                DateProgressionRecorded = DateTime.UtcNow,
                CurrentEmploymentStatus = ReferenceData.CurrentEmploymentStatus.NotKnown,
                LastModifiedTouchpointId = "0000000001",
                EmployerPostcode = "CV12 1CSXX"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEmployerAddressIsValid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                EconomicShockStatus = ReferenceData.EconomicShockStatus.NotApplicable,
                DateProgressionRecorded = DateTime.UtcNow,
                CurrentEmploymentStatus = ReferenceData.CurrentEmploymentStatus.NotKnown,
                LastModifiedTouchpointId = "0000000001",
                EmployerAddress = "10 Any Street, AnyTown"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEmployerAddressIsInvalid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                EconomicShockStatus = ReferenceData.EconomicShockStatus.NotApplicable,
                DateProgressionRecorded = DateTime.UtcNow,
                CurrentEmploymentStatus = ReferenceData.CurrentEmploymentStatus.NotKnown,
                LastModifiedTouchpointId = "0000000001",
                EmployerAddress = "10 Any Street, <AnyTown> & something else"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEmployerNameIsValid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                EconomicShockStatus = ReferenceData.EconomicShockStatus.NotApplicable,
                DateProgressionRecorded = DateTime.UtcNow,
                CurrentEmploymentStatus = ReferenceData.CurrentEmploymentStatus.NotKnown,
                LastModifiedTouchpointId = "0000000001",
                EmployerName = "Employer Name"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEmployerNameIsInvalid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                EconomicShockStatus = ReferenceData.EconomicShockStatus.NotApplicable,
                DateProgressionRecorded = DateTime.UtcNow,
                CurrentEmploymentStatus = ReferenceData.CurrentEmploymentStatus.NotKnown,
                LastModifiedTouchpointId = "0000000001",
                EmployerName = "[Employer Name]"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEconomicShockCodeIsValid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                EconomicShockStatus = ReferenceData.EconomicShockStatus.NotApplicable,
                DateProgressionRecorded = DateTime.UtcNow,
                CurrentEmploymentStatus = ReferenceData.CurrentEmploymentStatus.NotKnown,
                LastModifiedTouchpointId = "0000000001",
                EconomicShockCode = "Some relevant data here 18/09/2023"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEconomicShockCodeIsInvalid()
        {
            var employmentProgression = new Models.EmploymentProgressionPatch
            {
                CustomerId = Guid.NewGuid(),
                EmploymentProgressionId = Guid.NewGuid(),
                EconomicShockStatus = ReferenceData.EconomicShockStatus.NotApplicable,
                DateProgressionRecorded = DateTime.UtcNow,
                CurrentEmploymentStatus = ReferenceData.CurrentEmploymentStatus.NotKnown,
                LastModifiedTouchpointId = "0000000001",
                EconomicShockCode = "Some relevant data here <18/09/2023>"
            };

            var result = _validate.ValidateResource(employmentProgression);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }
    }
}

