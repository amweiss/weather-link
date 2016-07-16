using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeatherLink.Models;
using WeatherLink.Services;

namespace WeatherLink.Controllers {

    /// <summary>
    /// Provide traffic advice.
    /// </summary>
    [Route("trafficadvice")]
    public class TrafficAdviceController : Controller {
        readonly IOptions<WeatherLinkSettings> _optionsAccessor;
        readonly ITrafficAdviceService _trafficAdviceService;
        readonly IGeocodeService _geocodeService;
        readonly IDistanceToDurationService _distanceToDurationService;

        /// <summary>
        /// Access traffic advice via a web API.
        /// </summary>
        public TrafficAdviceController(IOptions<WeatherLinkSettings> optionsAccessor, ITrafficAdviceService trafficAdviceService, IGeocodeService geocodeService, IDistanceToDurationService distanceToDurationService) {
            _optionsAccessor = optionsAccessor;
            _trafficAdviceService = trafficAdviceService;
            _geocodeService = geocodeService;
            _distanceToDurationService = distanceToDurationService;
        }

        /// <summary>
        /// Get traffic advice based on a latitude and longitude.
        /// </summary>
        /// <param name="latitude">Latitude in degrees.</param>
        /// <param name="longitude">Longitude in degrees.</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [Route("{latitude}/{longitude}")]
        [HttpGet]
        public async Task<string> GetTrafficAdvice(double latitude, double longitude) {
            var result = await _trafficAdviceService.GetTrafficAdvice(latitude, longitude);
            if (result == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var retVal = new StringBuilder();
            retVal.AppendLine(result);
            retVal.AppendLine(GetForecastIoByLine(latitude, longitude));
            return retVal.ToString();
        }

        /// <summary>
        /// Get traffic advice for a geocoded location.
        /// </summary>
        /// <param name="location">The string to translate into latitude and longitude.</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [Route("{location}")]
        [HttpGet]
        public async Task<string> GetTrafficAdvice(string location) {
            var target = await _geocodeService.Geocode(location);
            if (target == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var result = await _trafficAdviceService.GetTrafficAdvice(target.Item1, target.Item2);
            if (result == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var retVal = new StringBuilder();
            retVal.AppendLine(result);
            retVal.AppendLine(GetForecastIoByLine(target.Item1, target.Item2));
            return retVal.ToString();
        }

        /// <summary>
        /// Get traffic advice for a geocoded location at a specific time.
        /// </summary>
        /// <param name="location">The string to translate into latitude and longitude.</param>
        /// <param name="time">The time in hours from now as decimal representation.</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [Route("fortime/{time}/{location}")]
        [HttpGet]
        public async Task<string> GetTrafficAdviceForATime(string location, double time) {
            var target = await _geocodeService.Geocode(location);
            if (target == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var result = await _trafficAdviceService.GetTrafficAdviceForATime(target.Item1, target.Item2, time);
            if (result == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var retVal = new StringBuilder();
            retVal.AppendLine(result);
            retVal.AppendLine(GetForecastIoByLine(target.Item1, target.Item2));
            return retVal.ToString();
        }

        /// <summary>
        /// Get traffic advice for a geocoded location to another geolocation. The destination is only used for travel duration currently.
        /// </summary>
        /// <param name="startingLocation">The starting location string to translate into latitude and longitude.</param>
        /// <param name="endingLocation">The ending location string to translate into latitude and longitude.</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [Route("from/{startingLocation}/to/{endingLocation}")]
        [HttpGet]
        public async Task<string> GetTrafficAdviceToALocation(string startingLocation, string endingLocation) {
            var durationTask = _distanceToDurationService.TimeInMinutesBetweenLocations(startingLocation, endingLocation);
            var targetTask = _geocodeService.Geocode(startingLocation);

            var target = await targetTask;
            var duration = await durationTask;

            if (duration == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            if (target == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var result = await _trafficAdviceService.GetTrafficAdvice(target.Item1, target.Item2, duration.Value);
            if (result == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var retVal = new StringBuilder();
            retVal.AppendLine(result);
            retVal.AppendLine(GetForecastIoByLine(target.Item1, target.Item2));
            return retVal.ToString();
        }

        /// <summary>
        /// Get traffic advice for a geocoded location to another geolocation at a specific time. The destination is only used for travel duration currently.
        /// </summary>
        /// <param name="startingLocation">The starting location string to translate into latitude and longitude.</param>
        /// <param name="endingLocation">The ending location string to translate into latitude and longitude.</param>
        /// <param name="time">The time in hours from now as decimal representation.</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [Route("fortime/{time}/from/{startingLocation}/to/{endingLocation}")]
        [HttpGet]
        public async Task<string> GetTrafficAdviceToALocationForATime(string startingLocation, string endingLocation, double time) {
            var durationTask = _distanceToDurationService.TimeInMinutesBetweenLocations(startingLocation, endingLocation);
            var targetTask = _geocodeService.Geocode(startingLocation);

            var targetResult = await targetTask;
            var durationResult = await durationTask;

            if (durationResult == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            if (targetResult == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var result = await _trafficAdviceService.GetTrafficAdviceForATime(targetResult.Item1, targetResult.Item2, time, durationResult.Value);
            if (result == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var retVal = new StringBuilder();
            retVal.AppendLine(result);
            retVal.AppendLine(GetForecastIoByLine(targetResult.Item1, targetResult.Item2));
            return retVal.ToString();
        }

        /// <summary>
        /// An endpoint for handling messages from slack.
        /// </summary>
        /// <param name="text">The slack message, it should match "^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$"</param>
        /// <param name="token">The slack token to verify it's a team that is setup in WeatherLinkSettings.SlackTokens.</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [Route("slack")]
        [HttpPost]
        public async Task<SlackResponse> SlackIntegration(string text, string token) {
            if (!_optionsAccessor.Value.SlackTokens.Contains(token)) {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return null;
            }

            var checkCommand = Regex.Match(text, @"^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$");

            var retVal = (string)null;

            if (checkCommand.Success) {
                var hours = checkCommand.Groups?[1]?.Value;
                var startingLocation = checkCommand.Groups?[2]?.Value;
                var endingLocation = checkCommand.Groups?[3]?.Value;

                var hoursFromNow = Convert.ToDouble(hours);

                if (hoursFromNow < 0 || startingLocation == null) {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return null;
                }

                // TODO: Find better way?
                if (endingLocation == null && hours != null) {
                    retVal = await GetTrafficAdviceForATime(startingLocation, hoursFromNow);
                }
                else if (endingLocation != null && hours == null) {
                    retVal = await GetTrafficAdviceToALocation(startingLocation, endingLocation);
                }
                else if (endingLocation != null && hours != null) {
                    retVal = await GetTrafficAdviceToALocationForATime(startingLocation, endingLocation, hoursFromNow);
                }
                else {
                    retVal = await GetTrafficAdvice(startingLocation);
                }
            }

            if (retVal == null) {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            return new SlackResponse { response_type = "in_channel", text = Regex.Replace(retVal, @"\r\n?|\n", "\n") };
        }

        private static string GetForecastIoByLine(double latitude, double longitude) => $"<http://forecast.io/#/f/{latitude:N4},{longitude:N4}|Powered by Forecast>";
    }
}