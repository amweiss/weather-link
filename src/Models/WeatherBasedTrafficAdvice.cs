using Humanizer;
using System;
using System.Text;

namespace WeatherLink.Models {

    /// <summary>
    /// A collection of data that provides advice based on weather data.
    /// </summary>
    public class WeatherBasedTrafficAdvice {
        /// <summary>
        /// The desired time to get adivce for.
        /// </summary>
        public DateTime? TargetTime { get; set; }

        /// <summary>
        /// The point in time and precipitation for the time closest to the current time.
        /// </summary>
        public Weather Currently { get; set; }

        /// <summary>
        /// The point in time that would be the best to leave based on the weather.
        /// </summary>
        public Weather BestTimeToLeave { get; set; }

        /// <summary>
        /// The point in time with the next measurable precipitation.
        /// </summary>
        public Weather NextPrecipitation { get; set; }

        /// <summary>
        /// The point in time with the minimum precipitation.
        /// </summary>
        public Weather MinimumPrecipitation { get; set; }

        /// <summary>
        /// The next point in time that has measurable precipitation after the next minimum level of precipitation.
        /// </summary>
        public Weather NextPrecipitationAfterMinimum { get; set; }

        /// <summary>
        /// The next point in time for moderate precipitation.
        /// </summary>
        public Weather NextModeratePrecipitation { get; set; }

        /// <summary>
        /// The next point in time for heavy precipitation.
        /// </summary>
        public Weather NextHeavyPrecipitation { get; set; }

        /// <summary>
        /// The data source.
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// Text to show where the data comes from.
        /// </summary>
        public string AttributionLine { get; set; }

        /// <summary>
        /// Convert the advice to a human readable string.
        /// </summary>
        /// <returns>Traffic advice in text format.</returns>
        public override string ToString() {
            var homeDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(Currently.time);

            if (TargetTime.HasValue) {
                var now = homeDateTimeOffset.UtcDateTime;
                return BestTimeToLeave != null 
                    ? $"The best time to leave about {TargetTime.Humanize(dateToCompareAgainst: now)} is {DateTimeOffset.FromUnixTimeSeconds(BestTimeToLeave.time).Humanize(homeDateTimeOffset)}."
                    : $"Clear skys around {TargetTime.Humanize(dateToCompareAgainst: now)} when you want to leave!";
            } else {
                var sb = new StringBuilder();
                if (Currently.precipIntensity > 0) {

                    if (BestTimeToLeave != null) {
                        sb.AppendLine($"The best time to leave in the next hour is {DateTimeOffset.FromUnixTimeSeconds(BestTimeToLeave.time).Humanize(homeDateTimeOffset)}.");
                    }

                    if (MinimumPrecipitation != null) {
                        sb.AppendLine($"{Currently.precipType.Transform(To.SentenceCase)} reaches lowest point {DateTimeOffset.FromUnixTimeSeconds(MinimumPrecipitation.time).Humanize(homeDateTimeOffset)}.");
                    }

                    if (NextPrecipitationAfterMinimum != null) {
                        sb.AppendLine($"{NextPrecipitationAfterMinimum.precipType.Transform(To.SentenceCase)} starting again after lowest point {DateTimeOffset.FromUnixTimeSeconds(NextPrecipitationAfterMinimum.time).Humanize(homeDateTimeOffset)}.");
                    }

                    if (NextModeratePrecipitation != null) {
                        sb.AppendLine($"{(NextModeratePrecipitation.precipType ?? Currently.precipType).Transform(To.SentenceCase)} getting worse {DateTimeOffset.FromUnixTimeSeconds(NextModeratePrecipitation.time).Humanize(homeDateTimeOffset)}.");
                    }

                    if (NextHeavyPrecipitation != null) {
                        sb.AppendLine($"{(NextHeavyPrecipitation.precipType ?? Currently.precipType).Transform(To.SentenceCase)} getting much worse {DateTimeOffset.FromUnixTimeSeconds(NextHeavyPrecipitation.time).Humanize(homeDateTimeOffset)}.");
                    }
                } else {
                    if (NextPrecipitation?.precipType != null) {
                        sb.AppendLine($"Next light {NextPrecipitation.precipType} is {DateTimeOffset.FromUnixTimeSeconds(NextPrecipitation.time).Humanize(homeDateTimeOffset)}.");
                    }
                    if (NextModeratePrecipitation?.precipType != null) {
                        sb.AppendLine($"Next moderate {NextModeratePrecipitation.precipType} is {DateTimeOffset.FromUnixTimeSeconds(NextModeratePrecipitation.time).Humanize(homeDateTimeOffset)}.");
                    }
                    if (NextHeavyPrecipitation?.precipType != null) {
                        sb.AppendLine($"Next heavy {NextHeavyPrecipitation.precipType} is {DateTimeOffset.FromUnixTimeSeconds(NextHeavyPrecipitation.time).Humanize(homeDateTimeOffset)}.");
                    }
                }

                return sb.Length != 0 ? sb.ToString() : "Clear skies for as far I can see!";
            }
        }
    }
}