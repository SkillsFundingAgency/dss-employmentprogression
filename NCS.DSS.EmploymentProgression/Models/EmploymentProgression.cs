using DFC.JSON.Standard.Attributes;
using DFC.Swagger.Standard.Annotations;
using NCS.DSS.EmploymentProgression.ReferenceData;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace NCS.DSS.EmploymentProgression.Models
{
    public class EmploymentProgression : IEmploymentProgression
    {
        private const string PostcodeRegEx = @"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z]))))\s?[0-9][A-Za-z]{2})";

        [Display(Description = "Unique identifier for a Employment Progression record")]
        [Example(Description = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85")]
        [JsonProperty(PropertyName = "id")]
        public Guid? EmploymentProgressionId { get; set; }

        [Display(Description = "Unique identifier of a customer")]
        [Example(Description = "2730af9c-fc34-4c2b-a905-c4b584b0f379")]
        [Required]
        public Guid? CustomerId { get; set; }

        [DataType(DataType.DateTime)]
        [Example(Description = "2018-06-21T17:45:00")]
        [Display(Description = "Date and time employment progression was created.")]
        [Required]
        public DateTime? DateProgressionRecorded { get; set; }

        [Display(Description = "Employment Status.")]
        [Example(Description = "3")]
        [Required]
        public CurrentEmploymentStatus? CurrentEmploymentStatus { get; set; }

        [Display(Description = "Economic shock status. Defaults to 3 if not provided")]
        [Example(Description = "3")]
        [Required]
        public EconomicShockStatus? EconomicShockStatus { get; set; }

        [RegularExpression(@"^[^<>]+$")]
        [Display(Description = "Economic Shock Code")]
        [Example(Description = "Thousands of employees made redundant at local car plant.")]
        [StringLength(50)]
        public string EconomicShockCode { get; set; }

        [RegularExpression(@"^[a-zA-Z]+(([\s'\,\.\-][a-zA-Z][0-9])?[a-zA-Z][0-9]*)*$")]
        [Display(Description = "Name of the employer")]
        [Example(Description = "Employer Limited")]
        [StringLength(200)]
        public string EmployerName { get; set; }

        [RegularExpression(@"^[^<>]+$")]
        [Display(Description = "Address of employer")]
        [Example(Description = "1 Employer Street, Coventry, West Midlands")]
        [StringLength(500)]
        public string EmployerAddress { get; set; }

        [StringLength(10)]
        [RegularExpression(PostcodeRegEx, ErrorMessage = "Please enter a valid postcode")]
        [Display(Description = "Employer postcode")]
        [Example(Description = "AA11AA")]
        public string EmployerPostcode { get; set; }

        [RegularExpression(@"^(\+|-)?(?:90(?:(?:\.0{1,6})?)|(?:[0-9]|[1-8][0-9])(?:(?:\.[0-9]{1,6})?))$")]
        [Display(Description = "Geocoded address information")]
        [Example(Description = "52.40100")]
        [JsonIgnoreOnSerialize]
        public decimal? Latitude { get; set; }

        [RegularExpression(@"^(\+|-)?(?:180(?:(?:\.0{1,6})?)|(?:[0-9]|[1-9][0-9]|1[0-7][0-9])(?:(?:\.[0-9]{1,6})?))$")]
        [Display(Description = "Geocoded address information")]
        [Example(Description = "-1.50812")]
        [JsonIgnoreOnSerialize]
        public decimal? Longitude { get; set; }

        [Display(Description = "Employment Hours.")]
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

        [Display(Description = "Length of unemployment")]
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
