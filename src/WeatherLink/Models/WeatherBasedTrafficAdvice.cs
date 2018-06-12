using DarkSky.Models;
using Humanizer;
using System;
using System.Text;

namespace WeatherLink.Models
{
	/// <summary>
	/// A collection of data that provides advice based on weather data.
	/// </summary>
	public class WeatherBasedTrafficAdvice
	{
		/// <summary>
		/// Text to show where the data comes from.
		/// </summary>
		public string AttributionLine { get; set; }

		/// <summary>
		/// The point in time that would be the best to leave based on the weather.
		/// </summary>
		public DataPoint BestTimeToLeave { get; set; }

		/// <summary>
		/// The point in time and precipitation for the time closest to the current time.
		/// </summary>
		public DataPoint Currently { get; set; }

		/// <summary>
		/// The data source.
		/// </summary>
		public string DataSource { get; set; }

		/// <summary>
		/// The point in time with the minimum precipitation.
		/// </summary>
		public DataPoint MinimumPrecipitation { get; set; }

		/// <summary>
		/// The next point in time for heavy precipitation.
		/// </summary>
		public DataPoint NextHeavyPrecipitation { get; set; }

		/// <summary>
		/// The next point in time for moderate precipitation.
		/// </summary>
		public DataPoint NextModeratePrecipitation { get; set; }

		/// <summary>
		/// The point in time with the next measurable precipitation.
		/// </summary>
		public DataPoint NextPrecipitation { get; set; }

		/// <summary>
		/// The next point in time that has measurable precipitation after the next minimum level of precipitation.
		/// </summary>
		public DataPoint NextPrecipitationAfterMinimum { get; set; }

		/// <summary>
		/// The desired time to get adivce for.
		/// </summary>
		public DateTime? TargetTime { get; set; }

		/// <summary>
		/// Convert the advice to a human readable string.
		/// </summary>
		/// <returns>Traffic advice in text format.</returns>
		public override string ToString()
		{
			var homeDateTimeOffset = Currently.DateTime;

			if (TargetTime.HasValue)
			{
				var now = homeDateTimeOffset.UtcDateTime;
				return BestTimeToLeave == null
					? $"Clear skys around {TargetTime.Humanize(dateToCompareAgainst: now)} when you want to leave!"
					: $"The best time to leave about {TargetTime.Humanize(dateToCompareAgainst: now)} is {BestTimeToLeave.DateTime.Humanize(homeDateTimeOffset)}.";
			}
			else
			{
				var sb = new StringBuilder();
				if (Currently.PrecipIntensity > 0)
				{
					if (BestTimeToLeave != null)
					{
						sb.AppendLine($"The best time to leave in the next hour is {BestTimeToLeave.DateTime.Humanize(homeDateTimeOffset)}.");
					}

					if (MinimumPrecipitation != null)
					{
						sb.AppendLine($"{Currently.PrecipType.ToString().Transform(To.SentenceCase)} reaches lowest point {MinimumPrecipitation.DateTime.Humanize(homeDateTimeOffset)}.");
					}

					if (NextModeratePrecipitation != null)
					{
						var precipType = (NextModeratePrecipitation.PrecipType == PrecipitationType.None ? Currently.PrecipType : NextModeratePrecipitation.PrecipType);
						sb.AppendLine($"{precipType.ToString().Transform(To.SentenceCase)} getting worse {NextModeratePrecipitation.DateTime.Humanize(homeDateTimeOffset)}.");
					}

					if (NextHeavyPrecipitation != null)
					{
						var precipType = (NextHeavyPrecipitation.PrecipType == PrecipitationType.None ? Currently.PrecipType : NextHeavyPrecipitation.PrecipType);
						sb.AppendLine($"{precipType.ToString().Transform(To.SentenceCase)} getting much worse {NextHeavyPrecipitation.DateTime.Humanize(homeDateTimeOffset)}.");
					}

					if (NextPrecipitationAfterMinimum != null)
					{
						var precipType = (NextPrecipitationAfterMinimum.PrecipType == PrecipitationType.None ? Currently.PrecipType : NextPrecipitationAfterMinimum.PrecipType);
						sb.AppendLine($"{precipType.ToString().Transform(To.SentenceCase)} starting again after lowest point {NextPrecipitationAfterMinimum.DateTime.Humanize(homeDateTimeOffset)}.");
					}
				}
				else
				{
					if (NextPrecipitation?.PrecipType != null)
					{
						sb.AppendLine($"Next light {NextPrecipitation.PrecipType} is {NextPrecipitation.DateTime.Humanize(homeDateTimeOffset)}.");
					}
					if (NextModeratePrecipitation?.PrecipType != null)
					{
						sb.AppendLine($"Next moderate {NextModeratePrecipitation.PrecipType} is {NextModeratePrecipitation.DateTime.Humanize(homeDateTimeOffset)}.");
					}
					if (NextHeavyPrecipitation?.PrecipType != null)
					{
						sb.AppendLine($"Next heavy {NextHeavyPrecipitation.PrecipType} is {NextHeavyPrecipitation.DateTime.Humanize(homeDateTimeOffset)}.");
					}
				}

				return sb.Length != 0 ? sb.ToString() : "Clear skies for as far I can see!";
			}
		}
	}
}