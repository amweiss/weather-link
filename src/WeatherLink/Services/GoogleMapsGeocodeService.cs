// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.Services
{
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WeatherLink.Models;

    /// <summary>
    /// Provides a geocoding service based on the Google Maps API.
    /// </summary>
    public class GoogleMapsGeocodeService : IGeocodeService
    {
        private readonly IOptions<WeatherLinkSettings> optionsAccessor;

        /// <summary>
        /// Create a new Google Maps Geocode service based on optionsAccessor.
        /// </summary>
        /// <param name="optionsAccessor">The options to use for the service.</param>
        public GoogleMapsGeocodeService(IOptions<WeatherLinkSettings> optionsAccessor)
        {
            this.optionsAccessor = optionsAccessor;
        }

        /// <summary>
        /// Transform an address into a latitude and longitude.
        /// </summary>
        /// <param name="address">The location to turn into a latitude and longitude.</param>
        /// <returns>The Tuple of (latitude, longitude).</returns>
        public async Task<Tuple<double, double>> Geocode(string address)
        {
            using (var client = new HttpClient())
            {
                using var response = await client.GetAsync(new Uri($"{optionsAccessor.Value.GoogleMapsApiBase}maps/api/geocode/json?key={optionsAccessor.Value.GoogleMapsApiKey}&address={address}"));

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var responseJObject = JObject.Parse(responseJson);
                var location = responseJObject?["results"]?.First()?["geometry"]?["location"];
                var recievedLatitude = location?["lat"];
                var recievedLongitude = location?["lng"];
                if (double.TryParse(recievedLatitude?.ToString(), out var latitude) &&
                        double.TryParse(recievedLongitude?.ToString(), out var longitude))
                {
                    return Tuple.Create(latitude, longitude);
                }
            }

            return null;
        }
    }
}