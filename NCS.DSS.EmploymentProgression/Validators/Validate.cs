using NCS.DSS.EmploymentProgression.Enumerations;
using NCS.DSS.EmploymentProgression.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.EmploymentProgression.Validators
{
    public class Validate : IValidate
    {
        private ValidationContext _context;
        private List<ValidationResult> _results;
        private IEmploymentProgression _employmentProgressionResource;

        public List<ValidationResult> ValidateResource(IEmploymentProgression employmentProgressionResource)
        {
            _employmentProgressionResource = employmentProgressionResource ?? throw new ArgumentNullException(nameof(employmentProgressionResource));
            _context = new ValidationContext(employmentProgressionResource, null, null);
            _results = new List<ValidationResult>();

            Validator.TryValidateObject(employmentProgressionResource, _context, _results, true);
            ValidateEmploymentProgressionRules();

            return _results;
        }

        private void ValidateEmploymentProgressionRules()
        {
            ValidateDateProgressionRecorded();
            ValidateEmploymentStatus();
            ValidateEconomicShockStatus();
            ValidateEconomicShockCode();
            ValidateEmploymentHours();
            ValidateDateOfEmployment();
            ValidateDateOfLastEmployment();
            ValidateLengthOfUnemployment();
            ValidateLastModifiedDate();
        }

        private void ValidateDateProgressionRecorded()
        {
            if (_employmentProgressionResource.DateProgressionRecorded.HasValue)
            {
                if (_employmentProgressionResource.DateProgressionRecorded.Value > DateTime.UtcNow)
                {
                    _results.Add(new ValidationResult("Date And Time must be less the current date/time", new[] { "DateProgressionRecorded" }));
                }
            }
        }

        private void ValidateEmploymentStatus()
        {
            if (!Enum.IsDefined(typeof(CurrentEmploymentStatus), _employmentProgressionResource.CurrentEmploymentStatus))
            {
                _results.Add(new ValidationResult("Please supply a valid value for Current Employment Status", new[] { "CurrentEmploymentStatus" }));
            }
        }

        private void ValidateEconomicShockStatus()
        {
            if (!Enum.IsDefined(typeof(EconomicShockStatus), _employmentProgressionResource.EconomicShockStatus))
            {
                _results.Add(new ValidationResult("Please supply a valid value for Economic Shock Status", new[] { "EconomicShockStatus" }));
            }
        }

        private void ValidateEconomicShockCode()
        {
            if (_employmentProgressionResource.EconomicShockStatus == EconomicShockStatus.GovernmentDefinedEconomicShock)
            {
                if (string.IsNullOrEmpty(_employmentProgressionResource.EconomicShockCode))
                {
                    _results.Add(new ValidationResult("EconomicShockCode is required when Economic Shock Status is Government Defined Economic Shock.", new[] { "EconomicShockCode" }));
                }
            }
        }

        private void ValidateEmploymentHours()
        {
            if (_employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.Apprenticeship ||
                _employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.Employed ||
                _employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.EmployedAndVoluntaryWork ||
                _employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.RetiredAndVoluntaryWork ||
                _employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.SelfEmployed
                )
            {
                if (!_employmentProgressionResource.EmploymentHours.HasValue)
                {
                    _results.Add(new ValidationResult("Employment Hours is required when CurrentEmploymentStatus is Apprenticeship, Employed, EmployedAndVoluntaryWork, RetiredAndVoluntaryWork or SelfEmployed.", new[] { "EmploymentHours" }));
                }
            }
        }

        private void ValidateDateOfEmployment()
        {
            if (_employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.Apprenticeship ||
                _employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.Employed ||
                _employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.EmployedAndVoluntaryWork ||
                _employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.RetiredAndVoluntaryWork ||
                _employmentProgressionResource.CurrentEmploymentStatus == CurrentEmploymentStatus.SelfEmployed
                )
            {
                if (!_employmentProgressionResource.DateOfEmployment.HasValue)
                {
                    _results.Add(new ValidationResult("Date Of Employment is required when CurrentEmploymentStatus is Apprenticeship, Employed, EmployedAndVoluntaryWork, RetiredAndVoluntaryWork or SelfEmployed.", new[] { "EmploymentHours" }));
                }
            }
        }

        private void ValidateDateOfLastEmployment()
        {
            if (_employmentProgressionResource.DateOfLastEmployment.HasValue)
            {
                if (_employmentProgressionResource.DateOfLastEmployment.Value > DateTime.UtcNow)
                {
                    _results.Add(new ValidationResult("Date And Time must be less the current date/time", new[] { "DateProgressionRecorded" }));
                }
            }
        }

        private void ValidateLengthOfUnemployment()
        {
            if (_employmentProgressionResource.LengthOfUnemployment.HasValue)
            {
                if (!Enum.IsDefined(typeof(LengthOfUnemployment), _employmentProgressionResource.LengthOfUnemployment))
                {
                    _results.Add(new ValidationResult("Please supply a valid value for Length Of Unemployment.", new[] { "LengthOfUnemployment" }));
                }
            }
        }

        private void ValidateLastModifiedDate()
        {
            if (_employmentProgressionResource.LastModifiedDate.HasValue)
            {
                if (_employmentProgressionResource.LastModifiedDate.Value > DateTime.UtcNow)
                {
                    _results.Add(new ValidationResult("Date And Time must be less the current date/time", new[] { "LastModifiedDate" }));
                }
            }
        }
    }
}