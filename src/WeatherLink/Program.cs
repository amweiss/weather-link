using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
			var host = new WebHostBuilder()
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseIISIntegration()
				.UseStartup<Startup>()
				.Build();

			host.Run();
		}
	}
}