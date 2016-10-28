using System.Collections.Generic;
using System.Linq;

namespace DarkSky.Models
{
	/// <summary>
	/// Extensions for an IEnumerable of DataPoints
	/// </summary>
	public static class DataPointListExtensions
	{
		/// <summary>
		/// Finds the IEnumerable containing the DataPoints for the period of numberOfMinutes that contains the minimum amount of precipitation.
		/// </summary>
		/// <param name="forecasts">The IEnumerable of DataPoints to act upon.</param>
		/// <param name="numberOfMinutes">The period of minutes desired to contain the minimum amount of precipitation.</param>
		/// <returns></returns>
		public static IEnumerable<DataPoint> MinimumPrecipitation(this List<DataPoint> forecasts, int numberOfMinutes)
		{
			var currentSum = forecasts.Take(numberOfMinutes).Sum(x => x.PrecipIntensity);

			var minSum = currentSum;
			var minIndex = 0;
			var start = 1;
			var i = numberOfMinutes;

			while (start <= forecasts.Count - numberOfMinutes)
			{
				currentSum = currentSum - forecasts[start - 1].PrecipIntensity + forecasts[i].PrecipIntensity;

				if (currentSum < minSum)
				{
					minSum = currentSum;
					minIndex = start;
				}

				i++;
				start++;
			}

			return forecasts.Skip(minIndex).Take(numberOfMinutes);
		}
	}
}