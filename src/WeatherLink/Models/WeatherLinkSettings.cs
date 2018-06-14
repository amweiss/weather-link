using System.Collections.Generic;

namespace WeatherLink.Models
{
	/// <summary>
	/// Container for the application settings.
	/// </summary>
	public sealed class WeatherLinkSettings
	{
		/// <summary>
		/// The base URL for the Dark Sky API.
		/// </summary>
		public string DarkSkyApiBase { get; set; }

		/// <summary>
		/// The API key for use with the DarkSky API.
		/// </summary>
		public string DarkSkyApiKey { get; set; }

		/// <summary>
		/// The base URL for the Google Maps API.
		/// </summary>
		public string GoogleMapsApiBase { get; set; }

		/// <summary>
		/// The API key for use with the Google Maps API.
		/// </summary>
		public string GoogleMapsApiKey { get; set; }

		/// <summary>
		/// The base URL for the Slack API.
		/// </summary>
		public string SlackApiBase { get; set; }

		/// <summary>
		/// The ClientId for the Slack app.
		/// </summary>
		public string SlackClientId { get; set; }

		/// <summary>
		/// The Client secret for the Slack app.
		/// </summary>
		public string SlackClientSecret { get; set; }

		/// <summary>
		/// The verification token for Slack.
		/// </summary>
		public string SlackVerificationToken { get; set; }

		/// <summary>
		/// The connection string of the Slack token db.
		/// </summary>
		public string SlackTokenDbConnection { get; set; }
	}
}