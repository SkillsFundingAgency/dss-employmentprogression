﻿using DFC.JSON.Standard.Attributes;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.EmploymentProgression.ReferenceData;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.EmploymentProgression.Models
{
    public class EmploymentProgressionPatch : IEmploymentProgression
    {
        private const string PostcodeRegEx = @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z]))))\s?[0-9][A-Za-z]{2})";

        [Display(Description = "Unique identifier for a Employment Progression record")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [JsonProperty(PropertyName = "id")]
        [Required]
        public Guid? EmploymentProgressionId { get; set; }

        [Display(Description = "Unique identifier of a customer")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        [Required]
        public Guid? CustomerId { get; set; }

        [DataType(DataType.DateTime)]
        [Example(Description = "2018-06-21T17:45:00")]
        [Display(Description = "Date and time employment progression was created.")]
        public DateTime? DateProgressionRecorded { get; set; }

        [Display(Description = "Employment status.")]
        [Example(Description = "3")]
        public CurrentEmploymentStatus? CurrentEmploymentStatus { get; set; }

        [Display(Description = "Economic shock status")]
        [Example(Description = "2")]
        public EconomicShockStatus? EconomicShockStatus { get; set; }

        [RegularExpression(@"^[^<>]+$")]
        [Display(Description = "Economic Shock Code")]
        [Example(Description = "Thousands of employee made redundant at local car plant.")]
        public string EconomicShockCode { get; set; }

        [RegularExpression(@"^[a-zA-Z]+(([\s'\,\.\-][a-zA-Z])?[a-zA-Z]*)*$")]
        [Display(Description = "Name of the employer")]
        [Example(Description = "Employer Limited")]
        [StringLength(200)]
        public string EmployerName { get; set; }

        [RegularExpression(@"^[^<>]+$")]
        [Display(Description = "Address of employer")]
        [Example(Description = "1 Employer Street, Coventry, West Midlands")]
        [StringLength(500)]
        public string EmployerAddress { get; set; }

        [RegularExpression(PostcodeRegEx, ErrorMessage = "Please enter a valid postcode")]
        [Display(Description = "Postcode of the employer")]
        [Example(Description = "CV12 1CS")]
        [StringLength(10)]
        public string EmployerPostcode { get; set; }

        [Display(Description = "Latitude of the employer")]
        [Example(Description = "38.8951")]
        [JsonIgnoreOnSerialize]
        public decimal? Latitude { get; set; }

        [Display(Description = "Longitude of the employer")]
        [Example(Description = "-77.0364")]
        [JsonIgnoreOnSerialize]
        public decimal? Longitude { get; set; }

        [Display(Description = "Employment hours.")]
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

        [Display(Description = "Length of unemployment.")]
        [Example(Description = "2")]
        public LengthOfUnemployment? LengthOfUnemployment { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Description = "Date and time last modified.")]
        [Example(Description = "2018-06-21T17:45:00")]
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(10, MinimumLength = 10)]
        [RegularExpression(@"^[0-9]+$")]
        [Display(Description = "Identifier of the touchpoint who made the last change to the record")]
        [Example(Description = "0000000001")]
        public string LastModifiedTouchpointId { get; set; }

        [StringLength(10)]
        [JsonIgnoreOnSerialize]
        public string CreatedBy { get; set; }
    }
}
