using System.Threading.Tasks;

namespace WeatherLink.Services
{
    /// <summary>
    /// A service to get traffic advice.
    /// </summary>
    public interface ITrafficAdviceService
    {
        /// <summary>
        /// Get traffic advice for a given time and location.
        /// </summary>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        /// <param name="hoursFromNow">The hours from the current time to get traffice advice for.</param>
        /// <returns>A string of traffic advice.</returns>
        Task<string> GetTrafficAdviceForATime(double latitude, double longitude, double hoursFromNow);

        /// <summary>
        /// Get traffic advice for a given location.
        /// </summary>
        /// <param name="latitude">The latitude in degrees.</param>
        /// <param name="longitude">The longitude in degrees.</param>
        /// <returns>A string of traffic advice</returns>
        Task<string> GetTrafficAdvice(double latitude, double longitude);
    }
}