using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeatherLink.Models;
using WeatherLink.Services;

namespace WeatherLink.Controllers
{
	/// <summary>
	/// Provide traffic advice.
	/// </summary>
	[Route("api/[controller]")]
	public class TrafficAdviceController : Controller
	{
		private readonly IDistanceToDurationService _distanceToDurationService;
		private readonly IGeocodeService _geocodeService;
		private readonly IOptions<WeatherLinkSettings> _optionsAccessor;
		private readonly ITrafficAdviceService _trafficAdviceService;

		/// <summary>
		/// Access traffic advice via a web API.
		/// </summary>
		public TrafficAdviceController(IOptions<WeatherLinkSettings> optionsAccessor, ITrafficAdviceService trafficAdviceService, IGeocodeService geocodeService, IDistanceToDurationService distanceToDurationService)
		{
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
		[HttpGet("{latitude}/{longitude}")]
		public async Task<WeatherBasedTrafficAdvice> GetTrafficAdvice(double latitude, double longitude)
		{
			var result = await _trafficAdviceService.GetTrafficAdvice(latitude, longitude);
			if (result == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			return result;
		}

		/// <summary>
		/// Get traffic advice for a geocoded location.
		/// </summary>
		/// <param name="location">The string to translate into latitude and longitude.</param>
		/// <returns>A string value describing when to leave based on the weather.</returns>
		[HttpGet("{location}")]
		public async Task<WeatherBasedTrafficAdvice> GetTrafficAdvice(string location)
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

			return result;
		}

		/// <summary>
		/// Get traffic advice for a geocoded location at a specific time.
		/// </summary>
		/// <param name="location">The string to translate into latitude and longitude.</param>
		/// <param name="time">The time in hours from now as decimal representation.</param>
		/// <returns>A string value describing when to leave based on the weather.</returns>
		[HttpGet("fortime/{time}/{location}")]
		public async Task<WeatherBasedTrafficAdvice> GetTrafficAdviceForATime(string location, double time)
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

			return result;
		}

		/// <summary>
		/// Get traffic advice for a geocoded location to another geolocation. The destination is only used for travel duration currently.
		/// </summary>
		/// <param name="startingLocation">The starting location string to translate into latitude and longitude.</param>
		/// <param name="endingLocation">The ending location string to translate into latitude and longitude.</param>
		/// <returns>A string value describing when to leave based on the weather.</returns>
		[HttpGet("from/{startingLocation}/to/{endingLocation}")]
		public async Task<WeatherBasedTrafficAdvice> GetTrafficAdviceToALocation(string startingLocation, string endingLocation)
		{
			var durationTask = _distanceToDurationService.TimeInMinutesBetweenLocations(startingLocation, endingLocation);
			var duration = await durationTask;

			var targetTask = _geocodeService.Geocode(startingLocation);
			var target = await targetTask;

			if (duration == null || target == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			var result = await _trafficAdviceService.GetTrafficAdvice(target.Item1, target.Item2, duration.Value);
			if (result == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			return result;
		}

		/// <summary>
		/// Get traffic advice for a geocoded location to another geolocation at a specific time. The destination is only used for travel duration currently.
		/// </summary>
		/// <param name="startingLocation">The starting location string to translate into latitude and longitude.</param>
		/// <param name="endingLocation">The ending location string to translate into latitude and longitude.</param>
		/// <param name="time">The time in hours from now as decimal representation.</param>
		/// <returns>A string value describing when to leave based on the weather.</returns>
		[HttpGet("fortime/{time}/from/{startingLocation}/to/{endingLocation}")]
		public async Task<WeatherBasedTrafficAdvice> GetTrafficAdviceToALocationForATime(string startingLocation, string endingLocation, double time)
		{
			var durationTask = _distanceToDurationService.TimeInMinutesBetweenLocations(startingLocation, endingLocation);
			var durationResult = await durationTask;

			var targetTask = _geocodeService.Geocode(startingLocation);
			var targetResult = await targetTask;

			if (durationResult == null || targetResult == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			var result = await _trafficAdviceService.GetTrafficAdviceForATime(targetResult.Item1, targetResult.Item2, time, durationResult.Value);
			if (result == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			return result;
		}

		/// <summary>
		/// An endpoint for handling messages from slack.
		/// </summary>
		/// <param name="text">The slack message, it should match "^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$"</param>
		/// <param name="token">The slack token to verify it's a team that is setup in WeatherLinkSettings.SlackTokens.</param>
		/// <returns>A string value describing when to leave based on the weather.</returns>
		[HttpPost("slack")]
		public async Task<SlackResponse> SlackIntegration(string text, string token)
		{
			if (!_optionsAccessor.Value.SlackTokens.Contains(token))
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return null;
			}

			var advice = (WeatherBasedTrafficAdvice)null;

			var checkCommand = Regex.Match(text, @"^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$");

			if (checkCommand.Success)
			{
				var hours = checkCommand.Groups?[1]?.Value;
				var startingLocation = checkCommand.Groups?[2]?.Value;
				var endingLocation = checkCommand.Groups?[3]?.Value;

				var hasHours = double.TryParse(hours, out double hoursFromNow);

				if ((hasHours && hoursFromNow < 0) || startingLocation == null)
				{
					Response.StatusCode = (int)HttpStatusCode.BadRequest;
					return null;
				}

				if (string.IsNullOrWhiteSpace(endingLocation) && hasHours)
				{
					advice = await GetTrafficAdviceForATime(startingLocation, hoursFromNow);
				}
				else if (!string.IsNullOrWhiteSpace(endingLocation) && !hasHours)
				{
					advice = await GetTrafficAdviceToALocation(startingLocation, endingLocation);
				}
				else if (!string.IsNullOrWhiteSpace(endingLocation) && hasHours)
				{
					advice = await GetTrafficAdviceToALocationForATime(startingLocation, endingLocation, hoursFromNow);
				}
				else
				{
					advice = await GetTrafficAdvice(startingLocation);
				}
			}

			var message = (advice == null)
				? "An error occurred fetching current data."
				: $"{advice}{Environment.NewLine}<{advice.DataSource}|{advice.AttributionLine}>";

			Response.StatusCode = (int)HttpStatusCode.OK;
			return new SlackResponse { response_type = "in_channel", text = Regex.Replace(message, @"\r\n?|\n", "\n") };
		}
	}
}