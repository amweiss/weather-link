// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.Controllers
{
	using System.Net;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Options;
	using WeatherLink.Models;
	using WeatherLink.Services;

	/// <summary>
	/// Provide traffic advice.
	/// </summary>
	[Route("api/[controller]")]
	public class TrafficAdviceController : Controller
	{
		private readonly IDistanceToDurationService distanceToDurationService;
		private readonly IGeocodeService geocodeService;
		private readonly ITrafficAdviceService trafficAdviceService;

		/// <summary>
		/// Access traffic advice via a web API.
		/// </summary>
		/// <param name="distanceToDurationService">Service to convert a distance to a duration based on traffic.</param>
		/// <param name="geocodeService">Service to turn text into a geolocation.</param>
		/// <param name="trafficAdviceService">Service to get traffic advice.</param>
		public TrafficAdviceController(ITrafficAdviceService trafficAdviceService, IGeocodeService geocodeService, IDistanceToDurationService distanceToDurationService)
		{
			this.trafficAdviceService = trafficAdviceService ?? throw new System.ArgumentNullException(nameof(trafficAdviceService));
			this.geocodeService = geocodeService ?? throw new System.ArgumentNullException(nameof(geocodeService));
			this.distanceToDurationService = distanceToDurationService ?? throw new System.ArgumentNullException(nameof(distanceToDurationService));
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
			var result = await trafficAdviceService.GetTrafficAdvice(latitude, longitude);
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
			var target = await geocodeService.Geocode(location);
			if (target == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			var result = await trafficAdviceService.GetTrafficAdvice(target.Item1, target.Item2);
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
			var target = await geocodeService.Geocode(location);
			if (target == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			var result = await trafficAdviceService.GetTrafficAdviceForATime(target.Item1, target.Item2, time);
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
			var durationTask = distanceToDurationService.TimeInMinutesBetweenLocations(startingLocation, endingLocation);
			var duration = await durationTask;

			var targetTask = geocodeService.Geocode(startingLocation);
			var target = await targetTask;

			if (duration == null || target == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			var result = await trafficAdviceService.GetTrafficAdvice(target.Item1, target.Item2, duration.Value);
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
			var durationTask = distanceToDurationService.TimeInMinutesBetweenLocations(startingLocation, endingLocation);
			var durationResult = await durationTask;

			var targetTask = geocodeService.Geocode(startingLocation);
			var targetResult = await targetTask;

			if (durationResult == null || targetResult == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			var result = await trafficAdviceService.GetTrafficAdviceForATime(targetResult.Item1, targetResult.Item2, time, durationResult.Value);
			if (result == null)
			{
				Response.StatusCode = (int)HttpStatusCode.NoContent;
				return null;
			}

			return result;
		}
	}
}