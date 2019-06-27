// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.Models
{
    /// <summary>
    /// Container for the application settings.
    /// </summary>
    public sealed class WeatherLinkSettings
    {
        /// <summary>
        /// Gets or sets the base URL for the Dark Sky API.
        /// </summary>
        public string DarkSkyApiBase { get; set; }

        /// <summary>
        /// Gets or sets the API key for use with the DarkSky API.
        /// </summary>
        public string DarkSkyApiKey { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the Google Maps API.
        /// </summary>
        public string GoogleMapsApiBase { get; set; }

        /// <summary>
        /// Gets or sets the API key for use with the Google Maps API.
        /// </summary>
        public string GoogleMapsApiKey { get; set; }

        /// <summary>
        /// Gets or sets the base URL for the Slack API.
        /// </summary>
        public string SlackApiBase { get; set; }

        /// <summary>
        /// Gets or sets the secret to verify slack messages.
        /// </summary>
        public string SlackSigningSecret { get; set; }
    }
}