using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherLink.Models;

namespace WeatherLink
{
	/// <summary>
	/// Entry point class.
	/// </summary>
	public class Program
	{
		/// <summary>
		/// Program Entry point
		/// </summary>
		/// <param name="args">Program args</param>
        public static void Main(string[] args)
        {
			CreateHostBuilder(args).Build().Run();
        }

		/// <summary>
		/// Setup web hosting
		/// </summary>
		/// <param name="args">WebHost args</param>
		/// <returns>WebHost builder</returns>
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				//.UseApplicationInsights()
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}