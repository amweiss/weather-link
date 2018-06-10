using DarkSky.Models;
using System.Threading.Tasks;

namespace WeatherLink.Services
{
	/// <summary>
	/// A service to get a Dark Sky forecast for a latitude and longitude.
	/// </summary>
	public interface IDarkSkyService
	{
		/// <summary>
		/// Make a request to get forecast data.
		/// </summary>
		/// <param name="latitude">Latitude to request data for in decimal degrees.</param>
		/// <param name="longitude">Longitude to request data for in decimal degrees.</param>
		/// <returns>A DarkSkyResponse with the API headers and data.</returns>
		Task<DarkSkyResponse> GetForecast(double latitude, double longitude);
	}
}