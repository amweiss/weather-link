using System.Collections.Generic;
using System.Linq;
using WeatherLink.Models;

namespace WeatherLink.ExtensionMethods
{
    internal static class ListOfWeatherExtensions
    {
        internal static IEnumerable<Weather> MinimumPrecipitation(this List<Weather> forecasts, int numberOfMinutes)
        {
            var currentSum = forecasts.Take(numberOfMinutes).Sum(x => x.precipIntensity);

            var minSum = currentSum;
            var minIndex = 0;
            var start = 1;
            var i = numberOfMinutes;

            while (start <= forecasts.Count - numberOfMinutes)
            {
                currentSum = currentSum - forecasts[start - 1].precipIntensity + forecasts[i].precipIntensity;

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