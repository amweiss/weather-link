using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
			var host = CreateWebHostBuilder(args).Build();
			host.Run();
        }

		/// <summary>
		/// Setup web hosting
		/// </summary>
		/// <param name="args">WebHost args</param>
		/// <returns>WebHost builder</returns>
		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseApplicationInsights()
				.UseStartup<Startup>();
	}
}