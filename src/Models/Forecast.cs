using System.Collections.Generic;

namespace WeatherLink.Models {

    /// <summary>
    /// A collection of individual point in time weather objects.
    /// </summary>
    public class Forecast {

        /// <summary>
        /// The weather at the most current time possible.
        /// </summary>
        public Weather Currently { get; set; }

        /// <summary>
        /// The collection of future weather by the hour.
        /// </summary>
        public IEnumerable<Weather> HourlyData { get; set; }

        /// <summary>
        /// The collection of future weather by the minute.
        /// </summary>
        public IEnumerable<Weather> MinutelyData { get; set; }
    }
}