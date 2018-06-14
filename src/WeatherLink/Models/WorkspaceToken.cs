using System;

namespace WeatherLink.Models
{
	/// <summary>
	/// The token object from Slack.
	/// </summary>
	public class WorkspaceToken
	{
		/// <summary>
		/// A GUID to track the entries.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		/// The token to use to verify the instance.
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// The AppId making the request.
		/// </summary>
		public string AppId { get; set; }

		/// <summary>
		/// The team the app is installed to.
		/// </summary>
		public string TeamId { get; set; }
	}
}