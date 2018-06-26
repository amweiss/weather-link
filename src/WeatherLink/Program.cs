using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

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
		///
		public static void Main(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
				.UseStartup<Startup>()
				.Build()
				.Run();
	}
}