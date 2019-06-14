// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.Models
{
	using System.Text.Json.Serialization;

	/// <summary>
	/// A representation of what Slack can recieve on their webhook.
	/// </summary>
	public class SlackResponse
	{
		/// <summary>
		/// Gets or sets the type of the response that will be posted.
		/// </summary>
		[JsonPropertyName("response_type")]
		public string ResponseType { get; set; }

		/// <summary>
		/// Gets or sets the body of the response.
		/// </summary>
		[JsonPropertyName("text")]
		public string Text { get; set; }
	}
}