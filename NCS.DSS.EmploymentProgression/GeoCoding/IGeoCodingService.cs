using DFC.GeoCoding.Standard.OrdnanceSurvey.Models;

namespace NCS.DSS.EmploymentProgression.GeoCoding
{
    public interface IGeoCodingService
    {
        Task<Position> GetPositionForPostcodeAsync(string postcode);
    }
}
