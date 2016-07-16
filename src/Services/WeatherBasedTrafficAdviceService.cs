using Humanizer;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherLink.ExtensionMethods;
using WeatherLink.Models;

namespace WeatherLink.Services {

    class WeatherBasedTrafficAdviceService : ITrafficAdviceService {
        readonly IOptions<WeatherLinkSettings> _optionsAccessor;
        readonly IForecastService _forecastService;

        public WeatherBasedTrafficAdviceService(IOptions<WeatherLinkSettings> optionsAccessor, IForecastService forecastRepository) {
            _optionsAccessor = optionsAccessor;
            _forecastService = forecastRepository;
        }

        public async Task<string> GetTrafficAdviceForATime(double latitude, double longitude, double hoursFromNow, int travelTime) {
            var forecast = await _forecastService.GetForecast(latitude, longitude);
            var currently = forecast.Currently;

            if (forecast?.HourlyData == null) return "No data found";

            var forecasts = forecast.MinutelyData?.ToList();
            if (forecasts != null) {
                forecasts.AddRange(
                    forecast?.HourlyData.Where(x => !forecasts.Any() || (x.time > forecasts.Last().time && x.time > currently.time)));
            }
            else {
                forecasts = forecast.HourlyData.ToList();
            }

            var bestTimeToLeave = (Weather)null;
            var homeDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(currently.time);
            var now = homeDateTimeOffset.UtcDateTime;
            var targetTime = now.AddHours(hoursFromNow);

            if (targetTime - homeDateTimeOffset <= TimeSpan.FromHours(1)) {
                bestTimeToLeave = forecasts.Any() ? forecasts.MinimumPrecipitation(travelTime)?.FirstOrDefault() : null;
            }
            else {
                var targetUnixSeconds = new DateTimeOffset(targetTime).ToUnixTimeSeconds();
                var afterTarget = 0;

                while (afterTarget < forecasts.Count && forecasts[afterTarget].time < targetUnixSeconds) {
                    afterTarget++;
                }

                var range = forecasts.Skip(afterTarget - 2)
                    .Take(5)
                    .Where(x => Math.Abs((DateTimeOffset.FromUnixTimeSeconds(x.time) - targetTime).Hours) <= 1)
                    .ToList();

                if (!range.Any(x => x.precipIntensity > 0)) return $"Clear skys around {targetTime.Humanize(dateToCompareAgainst: now)} when you want to leave!";

                bestTimeToLeave = range.MinBy(x => x.precipIntensity);
            }

            return bestTimeToLeave == null ? "No result found" : $"The best time to leave about {targetTime.Humanize(dateToCompareAgainst: now)} is {DateTimeOffset.FromUnixTimeSeconds(bestTimeToLeave.time).Humanize(homeDateTimeOffset)}.";
        }

        public async Task<string> GetTrafficAdvice(double latitude, double longitude, int travelTime)
        //TODO: I hate this, fix it
        {
            var forecast = await _forecastService.GetForecast(latitude, longitude);
            var currently = forecast.Currently;
            var forecasts = forecast.MinutelyData?.ToList();

            if (currently == null || forecasts == null) return "No data found";

            var bestTimeToLeaveWork = forecasts.Any() ? forecasts.MinimumPrecipitation(travelTime)?.FirstOrDefault() : null;

            forecasts.AddRange(forecast.HourlyData.Where(x => !forecasts.Any() || (x.time > forecasts.Last().time && x.time > currently.time)));
            if (!forecasts.Any()) return "No data found";

            var nextModeratePrecipitation = forecasts.FirstOrDefault(x => x.precipIntensity >= Weather.ModerateThreshold && x.time >= currently.time);

            var nextHeavyPrecipitation = forecasts.FirstOrDefault(x => x.precipIntensity >= Weather.HeavyThreshold && x.time >= currently.time);

            var sb = new StringBuilder();
            var homeDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(currently.time);

            if (currently.precipIntensity > 0) {
                var minimumPrecipitation =
                    forecasts.FirstOrDefault(
                        x =>
                            x.precipIntensity.Equals(forecasts.Min(y => y.precipIntensity)) &&
                            x.time >= currently.time);

                var nextPrecipitationAfterMinimum =
                    forecasts.FirstOrDefault(
                        x =>
                            x.precipIntensity > Weather.MeasurableThreshold &&
                            x.precipIntensity > minimumPrecipitation?.precipIntensity &&
                            x.time >= minimumPrecipitation.time);

                if (bestTimeToLeaveWork != null) {
                    sb.AppendLine($"The best time to leave in the next hour is {DateTimeOffset.FromUnixTimeSeconds(bestTimeToLeaveWork.time).Humanize(homeDateTimeOffset)}.");
                }

                if (minimumPrecipitation != null) {
                    sb.AppendLine($"{currently.precipType.Transform(To.SentenceCase)} reaches lowest point {DateTimeOffset.FromUnixTimeSeconds(minimumPrecipitation.time).Humanize(homeDateTimeOffset)}.");
                }

                if (nextPrecipitationAfterMinimum != null) {
                    sb.AppendLine($"{nextPrecipitationAfterMinimum.precipType.Transform(To.SentenceCase)} starting again after lowest point {DateTimeOffset.FromUnixTimeSeconds(nextPrecipitationAfterMinimum.time).Humanize(homeDateTimeOffset)}.");
                }

                if (nextModeratePrecipitation != null) {
                    sb.AppendLine($"{(nextModeratePrecipitation.precipType ?? currently.precipType).Transform(To.SentenceCase)} getting worse {DateTimeOffset.FromUnixTimeSeconds(nextModeratePrecipitation.time).Humanize(homeDateTimeOffset)}.");
                }

                if (nextHeavyPrecipitation != null) {
                    sb.AppendLine($"{(nextHeavyPrecipitation.precipType ?? currently.precipType).Transform(To.SentenceCase)} getting much worse {DateTimeOffset.FromUnixTimeSeconds(nextHeavyPrecipitation.time).Humanize(homeDateTimeOffset)}.");
                }
            }
            else {
                var nextPrecipitation = forecasts.FirstOrDefault(x => x.precipIntensity > Weather.MeasurableThreshold && x.time >= currently.time);

                if (nextPrecipitation?.precipType != null) {
                    sb.AppendLine($"Next light {nextPrecipitation.precipType} is {DateTimeOffset.FromUnixTimeSeconds(nextPrecipitation.time).Humanize(homeDateTimeOffset)}.");
                }
                if (nextModeratePrecipitation?.precipType != null) {
                    sb.AppendLine($"Next moderate {nextModeratePrecipitation.precipType} is {DateTimeOffset.FromUnixTimeSeconds(nextModeratePrecipitation.time).Humanize(homeDateTimeOffset)}.");
                }
                if (nextHeavyPrecipitation?.precipType != null) {
                    sb.AppendLine($"Next heavy {nextHeavyPrecipitation.precipType} is {DateTimeOffset.FromUnixTimeSeconds(nextHeavyPrecipitation.time).Humanize(homeDateTimeOffset)}.");
                }
            }

            return sb.Length != 0 ? sb.ToString() : "Clear skies for as far I can see!";
        }
    }
}