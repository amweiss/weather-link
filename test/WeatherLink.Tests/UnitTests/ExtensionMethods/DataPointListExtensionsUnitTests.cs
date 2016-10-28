using Xunit;
using DarkSky.Models;
using System.Collections.Generic;
using System.Linq;

namespace WeatherLink.UnitTests.ExtensionMethods
{
    public class DataPointListExtensionsUnitTests
    {
        // Forecast _mockBuffaloData = JsonConvert.DeserializeObject<Forecast>(
        //     File.ReadAllText($"{AppContext.BaseDirectory}/Data/BexarTX.json"));

        List<DataPoint> _trivialData = new List<DataPoint>
        {
            new DataPoint{ Time = 0, PrecipIntensity = 6},
            new DataPoint{ Time = 1, PrecipIntensity = 7},
            new DataPoint{ Time = 2, PrecipIntensity = 8},
            new DataPoint{ Time = 3, PrecipIntensity = 2},
            new DataPoint{ Time = 4, PrecipIntensity = 0},
            new DataPoint{ Time = 5, PrecipIntensity = 6},
            new DataPoint{ Time = 6, PrecipIntensity = 7},
            new DataPoint{ Time = 7, PrecipIntensity = 8},
            new DataPoint{ Time = 8, PrecipIntensity = 9},
            new DataPoint{ Time = 9, PrecipIntensity = 0},
        };

        [Fact]
        public void EmptyListReturnsEmptyEnumerable()
        {
            var input = new List<DataPoint>();
            var result = input.MinimumPrecipitation(1);

            Assert.Empty(result);
        }

        [Fact]
        public void ZeroMinutesReturnsEmptyEnumerable()
        {
            var input = _trivialData;
            var result = input.MinimumPrecipitation(0);

            Assert.Empty(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ReturnsNonEmptyEnumerable(int value)
        {
            var input = _trivialData;
            var result = input.MinimumPrecipitation(value);

            Assert.NotEmpty(result);
        }

        [Fact]
        public void IncreasingListReturnsConsecutiveEnumerable()
        {
            var input = _trivialData.Take(3).ToList();
            var result = input.MinimumPrecipitation(2);

            Assert.Equal(result.ElementAt(0).Time, 0);
            Assert.Equal(result.ElementAt(1).Time, 1);
        }

        [Fact]
        public void NonIncreasingListReturnedWithinLargerSet()
        {
            var input = _trivialData;
            var result = input.MinimumPrecipitation(3);

            Assert.Equal(result.ElementAt(0).Time, 3);
            Assert.Equal(result.ElementAt(1).Time, 4);
            Assert.Equal(result.ElementAt(2).Time, 5);
        }
    }
}
