using DarkSky.Models;
using System.Collections.Generic;
using System.Linq;

namespace WeatherLink.ExtensionMethods
{
	internal static class ListOfWeatherExtensions
	{
		internal static IEnumerable<DataPoint> MinimumPrecipitation(this List<DataPoint> forecasts, int numberOfMinutes)
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