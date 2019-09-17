using System.ComponentModel;

namespace NCS.DSS.EmploymentProgression.ReferenceData
{
    public enum EconomicShockStatus
    {
        [Description("Local Economic Shock")]
        LocalEconomicShock = 1,

        [Description("Government Defined Economic Shock")]
        GovernmentDefinedEconomicShock = 2,

        [Description("Not Applicable")]
        NotApplicable = 3
    }
}