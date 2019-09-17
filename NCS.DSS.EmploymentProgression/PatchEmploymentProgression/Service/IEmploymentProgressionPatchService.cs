using NCS.DSS.EmploymentProgression.Models;

namespace NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service
{
    public interface IEmploymentProgressionPatchService
    {
        string PatchEmploymentProgressionAsync(string employmentProgressionAsJson, EmploymentProgressionPatch employmentProgressionPatch);
    }
}