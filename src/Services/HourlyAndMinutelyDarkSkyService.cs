using DarkSky.Models;
using DarkSky.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WeatherLink.Models;

namespace WeatherLink.Services
{
    /// <summary>
    /// A service to get a Dark Sky forecast for a latitude and longitude.
    /// </summary>
    public class HourlyAndMinutelyDarkSkyService : IDarkSkyService
    {
        private readonly DarkSkyService.OptionalParameters _darkSkyParameters = new DarkSkyService.OptionalParameters { DataBlocksToExclude = new List<string> { "daily", "alerts", "flags" } };
        private readonly DarkSkyService _darkSkyService;

        private class StandardHttpClient : DarkSky.Services.IHttpClient
        {
            readonly string _baseUri = String.Empty;
            public StandardHttpClient(string baseUri)
            {
                _baseUri = baseUri;
            }

            public async Task<HttpResponseMessage> HttpRequest(string requestString)
            {
                using (var handler = new HttpClientHandler())
                {
                    //if (handler.SupportsAutomaticDecompression)
                    //{
                    //    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    //}
                    using (var client = new HttpClient(handler))
                    {
                        client.BaseAddress = new Uri(_baseUri);
                        return await client.GetAsync(requestString);
                    }
                }
            }
        }

        /// <summary>
        /// An implementation of IDarkSkyService that exlcudes daily data, alert data, and flags data.
        /// </summary>
        /// <param name="optionsAccessor"></param>
        public HourlyAndMinutelyDarkSkyService(IOptions<WeatherLinkSettings> optionsAccessor)
        {
            _darkSkyService = new DarkSkyService(optionsAccessor.Value.DarkSkyApiKey, new StandardHttpClient("https://api.darksky.net/"));
        }

        /// <summary>
        /// Make a request to get forecast data.
        /// </summary>
        /// <param name="latitude">Latitude to request data for in decimal degrees.</param>
        /// <param name="longitude">Longitude to request data for in decimal degrees.</param>
        /// <returns>A DarkSkyResponse with the API headers and data.</returns>
        public async Task<DarkSkyResponse> GetForecast(double latitude, double longitude)
        {
            return await _darkSkyService.GetForecast(latitude, longitude, _darkSkyParameters);
        }
    }
}