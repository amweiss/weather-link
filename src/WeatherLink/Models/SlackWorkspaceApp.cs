using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace WeatherLink.Models
{
	/// <summary>
	/// Model for Slack tokens to keep that allow access.
	/// </summary>
	public class SlackWorkspaceAppContext : DbContext
	{
		private readonly IOptions<WeatherLinkSettings> _optionsAccessor;


		/// <summary>
		/// Constructor that takes options.
		/// </summary>
		public SlackWorkspaceAppContext(DbContextOptions<SlackWorkspaceAppContext> options, IOptions<WeatherLinkSettings> optionsAccessor)
				: base(options)
			{
				_optionsAccessor = optionsAccessor;
			}

		/// <summary>
		/// Setup the context.
		/// </summary>
		/// <param name="optionsBuilder">Context option builder</param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseCosmos(
				_optionsAccessor.Value.SlackTokenDbServiceEndpoint,
				_optionsAccessor.Value.SlackTokenDbAuthKey,
				_optionsAccessor.Value.SlackTokenDbDatabaseName
				);
		}

		/// <summary>
		/// The tokens from Slack.
		/// </summary>
		/// <returns></returns>
		public DbSet<WorkspaceToken> WorkspaceTokens { get; set; }
	}
}
