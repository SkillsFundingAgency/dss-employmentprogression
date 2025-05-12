using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.EmployeeProgression.GeoCoding
{
    public class GeoCodingService : IGeoCodingService
    {
        private readonly IAzureMapService _azureMapService;

        private readonly ILogger<GeoCodingService> _logger;

        public GeoCodingService(IAzureMapService azureMapService, ILogger<GeoCodingService> logger)
        {
            _azureMapService = azureMapService;
            _logger = logger;
        }

        public async Task<Position> GetPositionForPostcodeAsync(string postcode)
        {
            _logger.LogInformation("Attempting to Validate Postcode {PostCode}",postcode);
            if (string.IsNullOrEmpty(postcode))
                return null;
            try
            {
                _logger.LogInformation("Attempting to Get Position of Postcode {PostCode}", postcode);
                var position = await _azureMapService.GetPositionForAddress(postcode);
                if(position != null)
                {
                    _logger.LogInformation("Successfully Retrieved Position {Long}/{Lat} of Postcode {PostCode}",position.Lon,position.Lat, postcode);
                    return position;
                }
                _logger.LogWarning("Failed to Retrieve Position of Postcode {PostCode}", postcode);
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
