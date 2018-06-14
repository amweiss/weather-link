using Microsoft.EntityFrameworkCore;

namespace WeatherLink.Models
{
	/// <summary>
	/// Model for Slack tokens to keep that allow access.
	/// </summary>
	public class SlackWorkspaceAppContext : DbContext
	{
		/// <summary>
		/// Constructor that takes options.
		/// </summary>
		public SlackWorkspaceAppContext(DbContextOptions<SlackWorkspaceAppContext> options)
				: base(options)
			{ }

		/// <summary>
		/// The tokens from Slack.
		/// </summary>
		/// <returns></returns>
		public DbSet<WorkspaceToken> WorkspaceTokens { get; set; }
	}
}