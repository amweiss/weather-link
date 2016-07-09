using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeatherLink.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherLink.Services;

namespace WeatherLink.Controllers
{
    /// <summary>
    /// Provide traffic advice.
    /// </summary>
    [Route("trafficadvice")]
    public class TrafficAdviceController : Controller
    {
        private readonly IOptions<WeatherLinkSettings> _optionsAccessor;
        private readonly ITrafficAdviceService _trafficAdviceService;
        private readonly IGeocodeService _geocodeService;

        /// <summary>
        /// Access traffic advice via a web API.
        /// </summary>
        public TrafficAdviceController(IOptions<WeatherLinkSettings> optionsAccessor, ITrafficAdviceService trafficAdviceService, IGeocodeService geocodeService)
        {
            _optionsAccessor = optionsAccessor;
            _trafficAdviceService = trafficAdviceService;
            _geocodeService = geocodeService;
        }

        /// <summary>
        /// Get traffic advice based on a latitude and longitude.
        /// </summary>
        /// <param name="latitude">Latitude in degrees.</param>
        /// <param name="longitude">Longitude in degrees.</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [Route("{latitude}/{longitude}")]
        [HttpGet]
        public async Task<string> GetTrafficAdvice(double latitude, double longitude)
        {
            var result = await _trafficAdviceService.GetTrafficAdvice(latitude, longitude);
            if (result == null)
            {
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
        public async Task<string> GetTrafficAdvice(string location)
        {
            var target = await _geocodeService.Geocode(location);
            if (target == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var result = await _trafficAdviceService.GetTrafficAdvice(target.Item1, target.Item2);
            if (result == null)
            {
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
        public async Task<string> GetTrafficAdviceForTime(string location, double time)
        {
            var target = await _geocodeService.Geocode(location);
            if (target == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var result = await _trafficAdviceService.GetTrafficAdviceForATime(target.Item1, target.Item2, time);
            if (result == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            var retVal = new StringBuilder();
            retVal.AppendLine(result);
            retVal.AppendLine(GetForecastIoByLine(target.Item1, target.Item2));
            return retVal.ToString();
        }

        /// <summary>
        /// An endpoint for handling messages from slack.
        /// </summary>
        /// <param name="text">The slack message, it should match in (\d*[.,]?\d*) hour(s*) from (.*)</param>
        /// <param name="token">The slack token to verify it's a team that is setup in WeatherLinkSettings.SlackTokens.</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [Route("slack")]
        [HttpGet]
        public async Task<SlackResponse> SlackIntegration(string text, string token)
        {
            if (!_optionsAccessor.Value.SlackTokens.Contains(token))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return null;
            }

            var checkCommand = Regex.Match(text, @"in (\d*[.,]?\d*) hour(s*) from (.*)");

            var retVal = (string)null;

            if (checkCommand.Success)
            {
                var hours = checkCommand.Groups[1].Value;
                var location = checkCommand.Groups[3].Value;

                var targetAsDouble = Convert.ToDouble(hours);
                if (targetAsDouble < 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return null;
                }

                retVal = await GetTrafficAdviceForTime(location, targetAsDouble);
            }
            else
            {
                retVal = await GetTrafficAdvice(text);
            }

            if (retVal == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NoContent;
                return null;
            }

            return new SlackResponse { response_type = "in_channel", text = Regex.Replace(retVal, @"\r\n?|\n", "\n") };
        }

        private static string GetForecastIoByLine(double latitude, double longitude)
        {
            return $"<http://forecast.io/#/f/{latitude:N4},{longitude:N4}|Powered by Forecast>";
        }
    }
}