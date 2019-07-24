#region

using System.Text.Json.Serialization;

#endregion

namespace WeatherLink.Models
{
    /// <summary>
    ///     A representation of what Slack can recieve on their webhook.
    /// </summary>
    public class SlackResponse
    {
        /// <summary>
        ///     Gets or sets the type of the response that will be posted.
        /// </summary>
        [JsonPropertyName("response_type")]
        public string ResponseType { get; set; }

        /// <summary>
        ///     Gets or sets the body of the response.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}