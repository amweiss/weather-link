// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink
{
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Hosting;

	/// <summary>
	/// Entry point class.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Program Entry point.
		/// </summary>
		/// <param name="args">Program args.</param>
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		/// <summary>
		/// Setup web hosting.
		/// </summary>
		/// <param name="args">WebHost args.</param>
		/// <returns>WebHost builder.</returns>
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}