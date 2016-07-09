using System.Collections.Generic;

namespace WeatherLink.Models
{
    /// <summary>
    /// Container for the application settings.
    /// </summary>
    public sealed class WeatherLinkSettings
    {
        /// <summary>
        /// The API key for use with the DarkSky API.
        /// </summary>
        public string DarkSkyApiKey { get; set; }

        /// <summary>
        /// The API key for use with the Google Maps API.
        /// </summary>
        public string GoogleMapsApiKey { get; set; }

        /// <summary>
        /// The collection of tokens from Slack that are valid for this application to accept.
        /// </summary>
        public List<string> SlackTokens { get; set; }

        /// <summary>
        /// The base URL for the Dark Sky API.
        /// </summary>
        public string DarkSkyApiBase { get; set; }

        /// <summary>
        /// The base URL for the Google Maps API.
        /// </summary>
        public string GoogleMapsApiBase { get; set; }
    }
}