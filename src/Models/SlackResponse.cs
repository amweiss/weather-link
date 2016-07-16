namespace WeatherLink {

    /// <summary>
    /// A representation of what Slack can recieve on their webhook.
    /// </summary>
    public class SlackResponse {

        /// <summary>
        /// The type of the response that will be posted.
        /// </summary>
        public string response_type { get; set; }

        /// <summary>
        /// The body of the response.
        /// </summary>
        public string text { get; set; }
    }
}