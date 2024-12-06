namespace NCS.DSS.EmploymentProgression.Models
{
    public class Customer
    {
        public Guid? CustomerId { get; set; }

        public DateTime? DateOfRegistration { get; set; }

        public string Title { get; set; }

        public string GivenName { get; set; }

        public string FamilyName { get; set; }

        public DateTime? DateofBirth { get; set; }

        public string Gender { get; set; }

        public string UniqueLearnerNumber { get; set; }

        public bool? OptInUserResearch { get; set; }

        public bool? OptInMarketResearch { get; set; }

        public DateTime? DateOfTermination { get; set; }

        public string ReasonForTermination { get; set; }

        public string IntroducedBy { get; set; }

        public string IntroducedByAdditionalInfo { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        public string LastModifiedTouchpointId { get; set; }
    }
}
