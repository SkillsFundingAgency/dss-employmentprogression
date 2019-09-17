using NCS.DSS.EmploymentProgression.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.EmploymentProgression.Validators
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IEmploymentProgression employmentProgression);
    }
}
