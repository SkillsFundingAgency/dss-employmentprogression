using NCS.DSS.EmploymentProgression.ReferenceData;
using System;

namespace NCS.DSS.EmploymentProgression.Models
{
    public interface IEmploymentProgression
    {
        Guid? EmploymentProgressionId { get; set; }
        Guid? CustomerId { get; set; }
        DateTime? DateProgressionRecorded { get; set; }
        CurrentEmploymentStatus? CurrentEmploymentStatus { get; set; }
        string EconomicShockCode { get; set; }
        EconomicShockStatus? EconomicShockStatus { get; set; }
        string EmployerName { get; set; }
        string EmployerAddress { get; set; }
        string EmployerPostcode { get; set; }
        decimal? Latitude { get; set; }
        decimal? Longitude { get; set; }
        EmploymentHours? EmploymentHours { get; set; }
        DateTime? DateOfEmployment { get; set; }
        DateTime? DateOfLastEmployment { get; set; }
        LengthOfUnemployment? LengthOfUnemployment { get; set; }
        DateTime? LastModifiedDate { get; set; }
        string LastModifiedTouchpointId { get; set; }
        string CreatedBy { get; set; }
    }
}