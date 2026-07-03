using DFC.GeoCoding.Standard.OrdnanceSurvey.Models;
using DFC.GeoCoding.Standard.OrdnanceSurvey.Services;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.EmploymentProgression.GeoCoding
{
    public class GeoCodingService : IGeoCodingService
    {
        private readonly IOSService _OSService;
        private readonly ILogger<GeoCodingService> _logger;

        public GeoCodingService(IOSService OSService, ILogger<GeoCodingService> logger)
        {
            _OSService = OSService;
            _logger = logger;
        }

        public async Task<Position> GetPositionForPostcodeAsync(string postcode)
        {
            _logger.LogTrace("Attempting to Validate Postcode {PostCode}",postcode);
            if (string.IsNullOrEmpty(postcode))
                return null;
            try
            {
                _logger.LogTrace("Attempting to Get Position of Postcode {PostCode}", postcode);
                var position = await _OSService.GetPositionForPostcodeAsync(postcode);
                if(position != null)
                {
                    _logger.LogTrace("Successfully Retrieved Position {Long}/{Lat} of Postcode {PostCode}",position.Longitude, position.Latitude, postcode);
                    return position;
                }
                _logger.LogInformation("Failed to Retrieve Position of Postcode {PostCode}", postcode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Get Position of Postcode {PostCode}", postcode);
                throw;
            }
            
        }
    }
}
