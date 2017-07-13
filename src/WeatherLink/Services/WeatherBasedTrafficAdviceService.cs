using DarkSky.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using WeatherLink.Models;
using static MoreLinq.MoreEnumerable;

namespace WeatherLink.Services
{
	internal class WeatherBasedTrafficAdviceService : ITrafficAdviceService
	{
		/// <summary>
		/// The threshold where percipitation is deemed heavy.
		/// </summary>
		public const double HeavyThreshold = 0.4;

		/// <summary>
		/// The threshold where percipitation is deemed measurable.
		/// </summary>
		public const double MeasurableThreshold = 0.005;

		/// <summary>
		/// The threshold where percipitation is deemed moderate.
		/// </summary>
		public const double ModerateThreshold = 0.1;

		private readonly IDarkSkyService _darkSkyService;

		public WeatherBasedTrafficAdviceService(IDarkSkyService darkSkyService)
		{
			_darkSkyService = darkSkyService;
		}

		public async Task<WeatherBasedTrafficAdvice> GetTrafficAdvice(double latitude, double longitude, int travelTime)
		{
			var forecastResponse = await _darkSkyService.GetForecast(latitude, longitude);
			if (forecastResponse?.Response?.Currently == null || forecastResponse.Response.Flags.DarkskyUnavailable != null) return null;
			var forecast = forecastResponse.Response;

			var retVal = new WeatherBasedTrafficAdvice { Currently = forecast.Currently, DataSource = forecastResponse.DataSource, AttributionLine = forecastResponse.AttributionLine };
			var forecasts = forecast.Minutely?.Data?.ToList();

			if (retVal.Currently == null || forecasts == null) return retVal;

			retVal.BestTimeToLeave = forecasts.Any() ? forecasts.MinimumPrecipitation(travelTime)?.FirstOrDefault() : null;

			forecasts.AddRange(forecast.Hourly.Data.Where(x => !forecasts.Any() || (x.DateTime > forecasts.Last().DateTime && x.DateTime > retVal.Currently.DateTime)));
			if (!forecasts.Any()) return retVal;

			retVal.NextModeratePrecipitation = forecasts.FirstOrDefault(x => x.PrecipIntensity >= ModerateThreshold && x.DateTime >= retVal.Currently.DateTime);

			retVal.NextHeavyPrecipitation = forecasts.FirstOrDefault(x => x.PrecipIntensity >= HeavyThreshold && x.DateTime >= retVal.Currently.DateTime);

			if (retVal.Currently.PrecipIntensity > 0)
			{
				retVal.MinimumPrecipitation =
					forecasts
						.Where(x => x.DateTime >= retVal.Currently.DateTime)
						.MinBy(x => x.PrecipIntensity);

				retVal.NextPrecipitationAfterMinimum =
					forecasts.FirstOrDefault(
						x =>
							x.DateTime > retVal.MinimumPrecipitation?.DateTime &&
							x.PrecipIntensity > MeasurableThreshold
							);
			}
			else
			{
				retVal.NextPrecipitation = forecasts.FirstOrDefault(x => x.PrecipIntensity > MeasurableThreshold && x.DateTime >= retVal.Currently.DateTime);
			}

			return retVal;
		}

		public async Task<WeatherBasedTrafficAdvice> GetTrafficAdviceForATime(double latitude, double longitude, double hoursFromNow, int travelTime)
		{
			var forecastResponse = await _darkSkyService.GetForecast(latitude, longitude);
			if (forecastResponse?.Response?.Hourly == null) return null;

			var forecast = forecastResponse.Response;
			var retVal = new WeatherBasedTrafficAdvice { Currently = forecast.Currently, DataSource = forecastResponse?.DataSource, AttributionLine = forecastResponse?.AttributionLine };

			var forecasts = forecast.Minutely?.Data?.ToList();
			if (forecasts != null)
			{
				forecasts.AddRange(
					forecast?.Hourly.Data.Where(x => !forecasts.Any() || (x.DateTime > forecasts.Last().DateTime && x.DateTime > retVal.Currently.DateTime)));
			}
			else
			{
				forecasts = forecast.Hourly.Data.ToList();
			}

			var homeDateTimeOffset = retVal.Currently.DateTime;
			var now = homeDateTimeOffset.UtcDateTime;
			retVal.TargetTime = now.AddHours(hoursFromNow);

			if (retVal.TargetTime - homeDateTimeOffset <= TimeSpan.FromHours(1))
			{
				retVal.BestTimeToLeave = forecasts.Any() ? forecasts.MinimumPrecipitation(travelTime)?.FirstOrDefault() : null;
			}
			else
			{
				var targetDateTimeOffset = new DateTimeOffset(retVal.TargetTime.Value);
				var afterTarget = 0;

				while (afterTarget < forecasts.Count && forecasts[afterTarget].DateTime < targetDateTimeOffset)
				{
					afterTarget++;
				}

				var range = forecasts.Skip(afterTarget - 2)
					.Take(5)
					.Where(x => Math.Abs((x.DateTime - retVal.TargetTime.Value).Hours) <= 1)
					.ToList();

				if (!range.Any(x => x.PrecipIntensity > 0)) return retVal;

				retVal.BestTimeToLeave = range.MinBy(x => x.PrecipIntensity);
			}

			return retVal;
		}
	}
}