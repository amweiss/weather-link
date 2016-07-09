namespace WeatherLink.Models
{
    /// <summary>
    /// Weather conditions at a point in time.
    /// </summary>
    public class Weather
    {
        /// <summary>
        /// The threshold where percipitation is deemed measurable.
        /// </summary>
        public const double MeasurableThreshold = 0.005;

        /// <summary>
        /// The threshold where percipitation is deemed moderate.
        /// </summary>
        public const double ModerateThreshold = 0.1;

        /// <summary>
        /// The threshold where percipitation is deemed heavy.
        /// </summary>
        public const double HeavyThreshold = 0.4;

        /// <summary>
        /// The UNIX time (that is, seconds since midnight GMT on 1 Jan 1970)
        /// at which this data point occurs. minutely data points are always aligned to the top of the minute,
        /// hourly points to the top of the hour, and daily points to midnight of the day, all according to the local time zone.
        /// </summary>
        public long time { get; set; }

        /// <summary>
        /// A numerical value representing the average expected intensity
        /// (in inches of liquid water per hour) of precipitation occurring at the given time
        /// conditional on probability (that is, assuming any precipitation occurs at all).
        /// </summary>
        public double precipIntensity { get; set; }

        /// <summary>
        /// A string representing the type of precipitation occurring at the given time.
        /// If defined, this property will have one of the following values:
        /// rain, snow, or sleet (which applies to each of freezing rain, ice pellets, and “wintery mix”).
        /// (If precipIntensity is zero, then this property will not be defined.)
        /// </summary>
        public string precipType { get; set; }
    }
}