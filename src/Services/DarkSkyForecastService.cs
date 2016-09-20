using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WeatherLink.Models;

namespace WeatherLink.Services {

    class DarkSkyForecastService : IForecastService {
        readonly IOptions<WeatherLinkSettings> _optionsAccessor;

        readonly String _dataSource = "https://darksky.net/poweredby/";
        readonly String _attributionLine = "Powered by Dark Sky";

        public DarkSkyForecastService(IOptions<WeatherLinkSettings> optionsAccessor) {
            _optionsAccessor = optionsAccessor;
        }

        async Task<JObject> GetAsJObject(double latitude, double longitude) {
            using (var client = new HttpClient()) {
                client.BaseAddress = new Uri(_optionsAccessor.Value.DarkSkyApiBase);
                var response = await client.GetAsync($"forecast/{_optionsAccessor.Value.DarkSkyApiKey}/{latitude:N4},{longitude:N4}?exclude=daily,alerts");
                if (!response.IsSuccessStatusCode) return null;
                var responseJson = await response.Content.ReadAsStringAsync();
                return JObject.Parse(responseJson);
            }
        }

        public async Task<Forecast> GetForecast(double latitude, double longitude) {
            var parsedResponse = await GetAsJObject(latitude, longitude);

            var currently = JsonConvert.DeserializeObject<Weather>(parsedResponse?["currently"]?.ToString());
            IEnumerable<Weather> minutelyData = null;
            IEnumerable<Weather> hourlyData = null;

            var minutelyValues = parsedResponse?["minutely"]?["data"];
            if (minutelyValues != null) {
                minutelyData =
                    minutelyValues.Children()
                        .Select(value => JsonConvert.DeserializeObject<Weather>(value.ToString()));
            }

            var hourlyValues = parsedResponse?["hourly"]?["data"];
            if (hourlyValues != null) {
                hourlyData =
                    hourlyValues.Children()
                        .Select(value => JsonConvert.DeserializeObject<Weather>(value.ToString()));
            }

            return new Forecast {
                Currently = currently,
                MinutelyData = minutelyData,
                HourlyData = hourlyData,
                DataSource = _dataSource,
                AttributionLine = _attributionLine
            };
    }
    }
}