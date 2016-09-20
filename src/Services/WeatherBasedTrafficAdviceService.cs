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

        public async Task<WeatherBasedTrafficAdvice> GetTrafficAdviceForATime(double latitude, double longitude, double hoursFromNow, int travelTime) {
            var forecast = await _forecastService.GetForecast(latitude, longitude);
            if (forecast?.HourlyData == null) return null;

            var retVal = new WeatherBasedTrafficAdvice { Currently = forecast.Currently, DataSource = forecast.DataSource, AttributionLine = forecast.AttributionLine };

            var forecasts = forecast.MinutelyData?.ToList();
            if (forecasts != null) {
                forecasts.AddRange(
                    forecast?.HourlyData.Where(x => !forecasts.Any() || (x.time > forecasts.Last().time && x.time > retVal.Currently.time)));
            }
            else {
                forecasts = forecast.HourlyData.ToList();
            }

            var homeDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(retVal.Currently.time);
            var now = homeDateTimeOffset.UtcDateTime;
            retVal.TargetTime = now.AddHours(hoursFromNow);

            if (retVal.TargetTime - homeDateTimeOffset <= TimeSpan.FromHours(1)) {
                retVal.BestTimeToLeave = forecasts.Any() ? forecasts.MinimumPrecipitation(travelTime)?.FirstOrDefault() : null;
            }
            else {
                var targetUnixSeconds = new DateTimeOffset(retVal.TargetTime.Value).ToUnixTimeSeconds();
                var afterTarget = 0;

                while (afterTarget < forecasts.Count && forecasts[afterTarget].time < targetUnixSeconds) {
                    afterTarget++;
                }

                var range = forecasts.Skip(afterTarget - 2)
                    .Take(5)
                    .Where(x => Math.Abs((DateTimeOffset.FromUnixTimeSeconds(x.time) - retVal.TargetTime.Value).Hours) <= 1)
                    .ToList();

                if (!range.Any(x => x.precipIntensity > 0)) return retVal;

                retVal.BestTimeToLeave = range.MinBy(x => x.precipIntensity);
            }

            return retVal;
        }

        public async Task<WeatherBasedTrafficAdvice> GetTrafficAdvice(double latitude, double longitude, int travelTime)
        //TODO: I hate this, fix it
        {
            var forecast = await _forecastService.GetForecast(latitude, longitude);
            var retVal = new WeatherBasedTrafficAdvice { Currently = forecast.Currently, DataSource = forecast.DataSource, AttributionLine = forecast.AttributionLine };
            var forecasts = forecast.MinutelyData?.ToList();

            if (retVal.Currently == null || forecasts == null) return retVal;

            retVal.BestTimeToLeave = forecasts.Any() ? forecasts.MinimumPrecipitation(travelTime)?.FirstOrDefault() : null;

            forecasts.AddRange(forecast.HourlyData.Where(x => !forecasts.Any() || (x.time > forecasts.Last().time && x.time > retVal.Currently.time)));
            if (!forecasts.Any()) return retVal;

            retVal.NextModeratePrecipitation = forecasts.FirstOrDefault(x => x.precipIntensity >= Weather.ModerateThreshold && x.time >= retVal.Currently.time);

            retVal.NextHeavyPrecipitation = forecasts.FirstOrDefault(x => x.precipIntensity >= Weather.HeavyThreshold && x.time >= retVal.Currently.time);

            if (retVal.Currently.precipIntensity > 0) {
                retVal.MinimumPrecipitation =
                    forecasts.FirstOrDefault(
                        x =>
                            x.precipIntensity.Equals(forecasts.Min(y => y.precipIntensity)) &&
                            x.time >= retVal.Currently.time);

                retVal.NextPrecipitationAfterMinimum =
                    forecasts.FirstOrDefault(
                        x =>
                            x.precipIntensity > Weather.MeasurableThreshold &&
                            x.precipIntensity > retVal.MinimumPrecipitation?.precipIntensity &&
                            x.time >= retVal.MinimumPrecipitation.time);
            }
            else {
                retVal.NextPrecipitation = forecasts.FirstOrDefault(x => x.precipIntensity > Weather.MeasurableThreshold && x.time >= retVal.Currently.time);
            }

            return retVal;
        }
    }
}