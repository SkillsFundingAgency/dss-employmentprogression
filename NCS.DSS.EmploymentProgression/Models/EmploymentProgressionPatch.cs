using DFC.JSON.Standard.Attributes;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.EmploymentProgression.Enumerations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.EmploymentProgression.Models
{
    public class EmploymentProgressionPatch : IEmploymentProgression
    {

        [Display(Description = "Unique identifier for a Employment Progression record")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [JsonProperty(PropertyName = "id")]
        [Required]
        public Guid? EmploymentProgressionID { get; set; }

        [Display(Description = "Unique identifier of a customer")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        [Required]
        public Guid? CustomerId { get; set; }

        [Example(Description = "2018-06-21T17:45:00")]
        [Display(Description = "Date and time employment progression was created.")]
        public DateTime? DateProgressionRecorded { get; set; }

        [Display(Description = "Employment Status: " +
                                "1 - Apprenticeship     " +
                                "2 - Economically Inactive" +
                                "3 - Economically Inactive and voluntary work" +
                                "4 - Employed" +
                                "5 - Employed and voluntary work" +
                                "6 - Employed at risk of redundancy" +
                                "7 - Retired" +
                                "8 - Retired and voluntary work" +
                                "9 - Self employed" +
                                "10 - Self employed and voluntary work" +
                                "11 - Unemployed" +
                                "12 - Unemployed and voluntary work" +
                                "99 - Not known")]
        [Example(Description = "3")]
        public CurrentEmploymentStatus? CurrentEmploymentStatus { get; set; }

        [Display(Description = "1 - Local economic shock        " +
                       "2 - Government defined economic shock        " +
                       "3 - Not applicable")]
        [Example(Description = "2")]
        [Required]
        public EconomicShockStatus? EconomicShockStatus { get; set; }
        [Display(Description = "Economic Shock Code")]
        [Example(Description = "Thousands of employee made redundant at local car plant.")]
        public string EconomicShockCode { get; set; }

        [Display(Description = "Name of the employer")]
        [Example(Description = "Employer Limited")]
        [StringLength(200)]
        public string EmployerName { get; set; }

        [Display(Description = "Address of employer")]
        [Example(Description = "1 Employer Street, Coventry, West Midlands")]
        [StringLength(500)]
        public string EmployerAddress { get; set; }

        [Display(Description = "Postcode of the employer")]
        [Example(Description = "CV12 1CS")]
        [StringLength(10)]
        public string EmployerPostcode { get; set; }

        [Display(Description = "Latitude of the employer")]
        [Example(Description = "38.8951")]
        [JsonIgnoreOnSerialize]
        public double? Latitude { get; set; }

        [Display(Description = "Longitude of the employer")]
        [Example(Description = "-77.0364")]
        [JsonIgnoreOnSerialize]
        public double? Longitude { get; set; }

        [Display(Description = "Employment Hours:   " +
                        "1 - Less than 16 hours,  " +
                        "2 - 16 hours or more,  " +
                        "98 - Prefer not to say,  " +
                        "99 - Not known")]
        [Example(Description = "2")]
        public EmploymentHours? EmploymentHours { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of employment.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? DateOfEmployment { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time of last employment.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? DateOfLastEmployment { get; set; }

        [Display(Description = "1 - Less than 3 months     " +
                               "2 - 3-5 months     " +
                               "3 - 6-11 months     " +
                               "4 - 12-23 months     " +
                               "5 - 24-35 months     " +
                               "6 - over 36 months     " +
                               "98 - Prefer not to say     " +
                               "99 - Not known/not provided")]
        [Example(Description = "2")]
        public LengthOfUnemployment? LengthOfUnemployment { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time last modified.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointID { get; set; }

        [StringLength(10)]
        [JsonIgnoreOnSerialize]
        public string CreatedBy { get; set; }
    }
}
