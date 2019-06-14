// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.Models
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// Model for Slack tokens to keep that allow access.
    /// </summary>
    public class SlackWorkspaceAppContext : DbContext
    {
        private readonly IOptions<WeatherLinkSettings> optionsAccessor;

        /// <summary>
        /// Constructor that takes options.
        /// </summary>
        /// <param name="options"><see cref="DbContextOptions"/>.</param>
        /// <param name="optionsAccessor">Service to access options from startup.</param>
        public SlackWorkspaceAppContext(DbContextOptions<SlackWorkspaceAppContext> options, IOptions<WeatherLinkSettings> optionsAccessor)
                : base(options)
        {
            this.optionsAccessor = optionsAccessor;
        }

        /// <summary>
        /// Gets or sets the tokens from Slack.
        /// </summary>
        public DbSet<WorkspaceToken> WorkspaceTokens { get; set; }

        /// <summary>
        /// Setup the context.
        /// </summary>
        /// <param name="optionsBuilder">Context option builder.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseCosmos(
                optionsAccessor.Value.SlackTokenDbServiceEndpoint,
                optionsAccessor.Value.SlackTokenDbAuthKey,
                optionsAccessor.Value.SlackTokenDbDatabaseName);
    }
}
