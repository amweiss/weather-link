using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;

namespace WeatherLink
{
	/// <summary>
	/// Entry point class.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Entry point method.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
							.AddCommandLine(args)
							.AddEnvironmentVariables()
							.Build();

			var host = new WebHostBuilder()
				.ConfigureLogging((hostingContext, logging) =>
				{
					logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
					logging.AddConsole();
					logging.AddDebug();
				})
				.UseConfiguration(config)
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseIISIntegration()
				.UseStartup<Startup>()
				.Build();

			host.Run();
		}
	}
}