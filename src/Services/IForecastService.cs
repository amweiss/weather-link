using System.Threading.Tasks;
using WeatherLink.Models;

namespace WeatherLink.Services
{
    /// <summary>
    /// A service to get a weather forecast.
    /// </summary>
    public interface IForecastService
    {
        /// <summary>
        /// Get the current forecast at a given latitude and longitude.
        /// </summary>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        /// <returns>The forecast for the given location.</returns>
        Task<Forecast> GetForecast(double latitude, double longitude);
    }
}