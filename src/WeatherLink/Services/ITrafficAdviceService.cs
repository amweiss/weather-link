#region

using System.Threading.Tasks;
using WeatherLink.Models;

#endregion

namespace WeatherLink.Services
{
    /// <summary>
    ///     A service to get traffic advice.
    /// </summary>
    public interface ITrafficAdviceService
    {
        /// <summary>
        ///     Get traffic advice for a given location.
        /// </summary>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        /// <param name="travelTime">The duration of the trip normally in minutes.</param>
        /// <returns>A string of traffic advice.</returns>
        Task<WeatherBasedTrafficAdvice> GetTrafficAdvice(double latitude, double longitude, int travelTime = 20);

        /// <summary>
        ///     Get traffic advice for a given time and location.
        /// </summary>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        /// <param name="hoursFromNow">The hours from the current time to get traffice advice for.</param>
        /// <param name="travelTime">The duration of the trip normally in minutes.</param>
        /// <returns>A string of traffic advice.</returns>
        Task<WeatherBasedTrafficAdvice> GetTrafficAdviceForATime(double latitude, double longitude, double hoursFromNow,
            int travelTime = 20);
    }
}