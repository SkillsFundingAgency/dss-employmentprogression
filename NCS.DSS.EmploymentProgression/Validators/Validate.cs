using NCS.DSS.EmploymentProgression.ReferenceData;
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
                    _results.Add(new ValidationResult("DateProgressionRecorded must be less than or equal to now.", new[] { "DateProgressionRecorded" }));
                }
            }
        }

        private void ValidateEmploymentStatus()
        {
            if (_employmentProgressionResource.CurrentEmploymentStatus.HasValue)
            {
                if (!Enum.IsDefined(typeof(CurrentEmploymentStatus), _employmentProgressionResource.CurrentEmploymentStatus))
                {
                    _results.Add(new ValidationResult("CurrentEmploymentStatus must have a valid Employment Status.", new[] { "CurrentEmploymentStatus" }));
                }
            }
        }

        private void ValidateEconomicShockStatus()
        {
            if (_employmentProgressionResource.EconomicShockStatus.HasValue)
            {
                if (!Enum.IsDefined(typeof(EconomicShockStatus), _employmentProgressionResource.EconomicShockStatus))
                {
                    _results.Add(new ValidationResult("EconomicShockStatus must have a valid Economic Shock Status.", new[] { "EconomicShockStatus" }));
                }
            }
        }

        private void ValidateEconomicShockCode()
        {
            if (_employmentProgressionResource.EconomicShockStatus.HasValue && _employmentProgressionResource.EconomicShockStatus == EconomicShockStatus.GovernmentDefinedEconomicShock)
            {
                if (string.IsNullOrEmpty(_employmentProgressionResource.EconomicShockCode))
                {
                    _results.Add(new ValidationResult("EconomicShockCode must have a value when Government Defined Economic Shock.", new[] { "EconomicShockCode" }));
                }
            }
        }

        private void ValidateEmploymentHours()
        {
            if (_employmentProgressionResource.CurrentEmploymentStatus.HasValue)
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
                        _results.Add(new ValidationResult("EmploymentHours must have a value when CurrentEmploymentStatus is Apprenticeship, Employed, EmployedAndVoluntaryWork, RetiredAndVoluntaryWork or SelfEmployed.", new[] { "EmploymentHours" }));
                    }
                    else
                    {
                        if (!Enum.IsDefined(typeof(EmploymentHours), _employmentProgressionResource.EmploymentHours))
                        {
                            _results.Add(new ValidationResult("EmploymentHours must be a valid employment hours.", new[] { "EmploymentHours" }));
                        }
                    }
                }
                else
                {
                    if (!Enum.IsDefined(typeof(EmploymentHours), _employmentProgressionResource.EmploymentHours))
                    {
                        _results.Add(new ValidationResult("EmploymentHours must be a valid employment hours.", new[] { "EmploymentHours" }));
                    }
                }
            }
            else
            {
                if (_employmentProgressionResource.EmploymentHours.HasValue)
                {
                    if (!Enum.IsDefined(typeof(EmploymentHours), _employmentProgressionResource.EmploymentHours))
                    {
                        _results.Add(new ValidationResult("EmploymentHours must be a valid employment hours.", new[] { "EmploymentHours" }));
                    }
                }
            }
        }

        private void ValidateDateOfEmployment()
        {
            if (_employmentProgressionResource.CurrentEmploymentStatus.HasValue)
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
                        _results.Add(new ValidationResult("DateOfEmployment must have a value when CurrentEmploymentStatus is Apprenticeship, Employed, EmployedAndVoluntaryWork, RetiredAndVoluntaryWork or SelfEmployed.", new[] { "DateOfEmployment" }));
                    }
                    else
                    {
                        if (_employmentProgressionResource.DateOfEmployment.Value > DateTime.UtcNow)
                        {
                            _results.Add(new ValidationResult("DateOfEmployment must be less than or equal to now.", new[] { "DateOfEmployment" }));
                        }
                    }
                }
            }
            else
            {
                if (_employmentProgressionResource.DateOfEmployment.HasValue)
                {
                    if (_employmentProgressionResource.DateOfEmployment.Value > DateTime.UtcNow)
                    {
                        _results.Add(new ValidationResult("DateOfEmployment must be less than or equal to now.", new[] { "DateOfEmployment" }));
                    }
                }
            }
        }

        private void ValidateDateOfLastEmployment()
        {
            if (_employmentProgressionResource.DateOfLastEmployment.HasValue)
            {
                if (_employmentProgressionResource.DateOfLastEmployment.Value > DateTime.UtcNow)
                {
                    _results.Add(new ValidationResult("DateOfLastEmployment must be less than or equal to now.", new[] { "DateOfLastEmployment" }));
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
                    _results.Add(new ValidationResult("LastModifiedDate must be less than or equal to now.", new[] { "LastModifiedDate" }));
                }
            }
        }
    }
}