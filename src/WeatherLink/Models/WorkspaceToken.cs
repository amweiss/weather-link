#region

using System;

#endregion

namespace WeatherLink.Models
{
    /// <summary>
    ///     The token object from Slack.
    /// </summary>
    public class WorkspaceToken
    {
        /// <summary>
        ///     Gets or sets a GUID to track the entries.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the token to use to verify the instance.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        ///     Gets or sets the AppId making the request.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        ///     Gets or sets the team the app is installed to.
        /// </summary>
        public string TeamId { get; set; }
    }
}