using System.Threading.Tasks;

namespace WeatherLink.Services {

    /// <summary>
    /// A service for providing travel duration based on distances.
    /// </summary>
    public interface IDistanceToDurationService {

        /// <summary>
        /// Find the travel time in minutes between two geocoded list.
        /// </summary>
        /// <param name="startingLocation">The starting location string to attempt to convert into a latitude and longitude.</param>
        /// <param name="endingLocation">The ending location string to attempt to convert into a latitude and longitude.</param>
        /// <returns>The duration of travel in minutes.</returns>
        Task<int?> TimeInMinutesBetweenLocations(string startingLocation, string endingLocation);
    }
}