using DFC.JSON.Standard;
using NCS.DSS.EmploymentProgression.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.EmploymentProgression.PatchEmploymentProgression.Service
{
    public class EmploymentProgressionPatchService : IEmploymentProgressionPatchService
    {
        private readonly IJsonHelper _jsonHelper;

        public EmploymentProgressionPatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }

        public string PatchEmploymentProgressionAsync(string employmentProgressionAsJson, EmploymentProgressionPatch employmentProgressionPatch)
        {
            try
            {
                var employmentProgressionAsJsonObject = JObject.Parse(employmentProgressionAsJson);

                if (employmentProgressionPatch.DateProgressionRecorded.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["DateProgressionRecorded"], employmentProgressionPatch.DateProgressionRecorded);

                if (employmentProgressionPatch.CurrentEmploymentStatus.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["CurrentEmploymentStatus"], employmentProgressionPatch.CurrentEmploymentStatus);

                if (employmentProgressionPatch.EconomicShockStatus.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EconomicShockStatus"], employmentProgressionPatch.EconomicShockStatus);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.EconomicShockCode))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EconomicShockCode"], employmentProgressionPatch.EconomicShockCode);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.EmployerName))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EmployerName"], employmentProgressionPatch.EmployerName);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.EmployerAddress))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EmployerAddress"], employmentProgressionPatch.EmployerAddress);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.EmployerPostcode))
                {
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EmployerPostcode"], employmentProgressionPatch.EmployerPostcode);

                    if (employmentProgressionPatch.Latitude.HasValue)
                        _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["Latitude"], employmentProgressionPatch.Latitude);

                    if (employmentProgressionPatch.Longitude.HasValue)
                        _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["Longitude"], employmentProgressionPatch.Longitude);
                }

                if (employmentProgressionPatch.Latitude.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["Latitude"], employmentProgressionPatch.Latitude);

                if (employmentProgressionPatch.Longitude.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["Longitude"], employmentProgressionPatch.Longitude);

                if (employmentProgressionPatch.DateOfEmployment.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["DateOfEmployment"], employmentProgressionPatch.DateOfEmployment);

                if (employmentProgressionPatch.DateOfLastEmployment.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["DateOfLastEmployment"], employmentProgressionPatch.DateOfLastEmployment);

                if (employmentProgressionPatch.LengthOfUnemployment.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["LengthOfUnemployment"], employmentProgressionPatch.LengthOfUnemployment);

                if (employmentProgressionPatch.LastModifiedDate.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["LastModifiedDate"], employmentProgressionPatch.LastModifiedDate);

                if (!string.IsNullOrEmpty(employmentProgressionPatch.LastModifiedTouchpointId))
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["LastModifiedTouchpointId"], employmentProgressionPatch.LastModifiedTouchpointId);

                if (employmentProgressionPatch.EmploymentHours.HasValue)
                    _jsonHelper.UpdatePropertyValue(employmentProgressionAsJsonObject["EmploymentHours"], employmentProgressionPatch.EmploymentHours);

                return employmentProgressionAsJsonObject.ToString();
            }
            catch (JsonReaderException)
            {
                throw;
            }
        }

    }
}
