#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DarkSky.Models;
using DarkSky.Services;
using Microsoft.Extensions.Options;
using WeatherLink.Models;

#endregion

namespace WeatherLink.Services
{
    /// <summary>
    ///     A service to get a Dark Sky forecast for a latitude and longitude.
    /// </summary>
    public class HourlyAndMinutelyDarkSkyService : IDarkSkyService, IDisposable
    {
        private readonly OptionalParameters darkSkyParameters = new OptionalParameters
        {
            DataBlocksToExclude = new List<ExclusionBlocks> {ExclusionBlocks.Daily, ExclusionBlocks.Alerts}
        };

        private readonly DarkSkyService darkSkyService;

        /// <summary>
        ///     An implementation of IDarkSkyService that exlcudes daily data, alert data, and flags data.
        /// </summary>
        /// <param name="optionsAccessor">Service to access options from startup.</param>
        public HourlyAndMinutelyDarkSkyService(IOptions<WeatherLinkSettings> optionsAccessor)
        {
            darkSkyService = new DarkSkyService(optionsAccessor?.Value.DarkSkyApiKey);
        }

        /// <summary>
        ///     Make a request to get forecast data.
        /// </summary>
        /// <param name="latitude">Latitude to request data for in decimal degrees.</param>
        /// <param name="longitude">Longitude to request data for in decimal degrees.</param>
        /// <returns>A DarkSkyResponse with the API headers and data.</returns>
        public async Task<DarkSkyResponse> GetForecast(double latitude, double longitude) =>
            await darkSkyService.GetForecast(latitude, longitude, darkSkyParameters);

        #region IDisposable Support

        private bool disposedValue; // To detect redundant calls

        /// <summary>
        ///     Dispose of resources used by the class.
        /// </summary>
        /// <param name="disposing">If the class is disposing managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    darkSkyService.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        ///     Public access to start disposing of the class instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}