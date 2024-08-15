﻿using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.GeoCoding.Standard.AzureMaps.Service;
using System.Threading.Tasks;

namespace NCS.DSS.EmployeeProgression.GeoCoding
{
    public class GeoCodingService : IGeoCodingService
    {
        private readonly IAzureMapService _azureMapService;

        public GeoCodingService(IAzureMapService azureMapService)
        {
            _azureMapService = azureMapService;
        }

        public async Task<Position> GetPositionForPostcodeAsync(string postcode)
        {
            if (string.IsNullOrEmpty(postcode))
                return null;

            return await _azureMapService.GetPositionForAddress(postcode);
        }
    }
}
