// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.UnitTests.ExtensionMethods
{
    using DarkSky.Models;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Unit tests for a List of <see cref="DataPoint"/>.
    /// </summary>
    public class DataPointListExtensionsUnitTests
    {
        // Forecast _mockBuffaloData = JsonConvert.DeserializeObject<Forecast>(File.ReadAllText($"{AppContext.BaseDirectory}/Data/BexarTX.json"));
        private readonly List<DataPoint> trivialData = new List<DataPoint>
        {
            new DataPoint { PrecipIntensity = 6 },
            new DataPoint { PrecipIntensity = 7 },
            new DataPoint { PrecipIntensity = 8 },
            new DataPoint { PrecipIntensity = 2 },
            new DataPoint { PrecipIntensity = 0 },
            new DataPoint { PrecipIntensity = 6 },
            new DataPoint { PrecipIntensity = 7 },
            new DataPoint { PrecipIntensity = 8 },
            new DataPoint { PrecipIntensity = 9 },
            new DataPoint { PrecipIntensity = 0 },
        };

        [Fact]
        public void EmptyListReturnsEmptyEnumerable()
        {
            var input = new List<DataPoint>();
            var result = input.MinimumPrecipitation(1);

            Assert.Empty(result);
        }

        [Fact]
        public void IncreasingListReturnsConsecutiveEnumerable()
        {
            var input = trivialData.Take(3).ToList();
            var result = input.MinimumPrecipitation(2);

            Assert.Equal(6, result.ElementAt(0).PrecipIntensity);
            Assert.Equal(7, result.ElementAt(1).PrecipIntensity);
        }

        [Fact]
        public void NonIncreasingListReturnedWithinLargerSet()
        {
            var input = trivialData;
            var result = input.MinimumPrecipitation(3);

            Assert.Equal(2, result.ElementAt(0).PrecipIntensity);
            Assert.Equal(0, result.ElementAt(1).PrecipIntensity);
            Assert.Equal(6, result.ElementAt(2).PrecipIntensity);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ReturnsNonEmptyEnumerable(int value)
        {
            var input = trivialData;
            var result = input.MinimumPrecipitation(value);

            Assert.NotEmpty(result);
        }

        [Fact]
        public void ZeroMinutesReturnsEmptyEnumerable()
        {
            var input = trivialData;
            var result = input.MinimumPrecipitation(0);

            Assert.Empty(result);
        }
    }
}