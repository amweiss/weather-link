using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using WeatherLink.Models;

namespace WeatherLink.Services
{
    /// <summary>
    /// A service for providing travel duration based on the Google Maps Distance Matrix API.
    /// </summary>
    public class GoogleMapsDistanceToDurationService : IDistanceToDurationService
    {
        private IOptions<WeatherLinkSettings> _optionsAccessor;

        /// <summary>
        /// Create a new Google Maps Distance to Duration service based on optionsAccessor.
        /// </summary>
        /// <param name="optionsAccessor">The options to use for the service.</param>
        public GoogleMapsDistanceToDurationService(IOptions<WeatherLinkSettings> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        /// <summary>
        /// Find the travel time in minutes between two geocoded list.
        /// </summary>
        /// <param name="startingLocation">The starting location string to attempt to convert into a latitude and longitude.</param>
        /// <param name="endingLocation">The ending location string to attempt to convert into a latitude and longitude.</param>
        /// <returns>The duration of travel by car in minutes based on the Google Maps Distance Matrix API.</returns>
        public async Task<int?> TimeInMinutesBetweenLocations(string startingLocation, string endingLocation)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_optionsAccessor.Value.GoogleMapsApiBase);
                var response = await client.GetAsync($"maps/api/distancematrix/json?units=imperial&origins={startingLocation}&destinations={endingLocation}&key={_optionsAccessor.Value.GoogleMapsApiKey}");
                if (!response.IsSuccessStatusCode) return null;
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseJObject = JObject.Parse(responseJson);
                var parsedDuration = responseJObject?["rows"]?.First()?["elements"]?.First()?["duration"]?["value"];
                double duration;
                if (double.TryParse(parsedDuration?.ToString(), out duration))
                {
                    return (int)(duration/60.0);
                }
            }

            return null;
        }
    }
}