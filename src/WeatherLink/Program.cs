#region

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

#endregion

namespace WeatherLink
{
    /// <summary>
    ///     Entry point class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     Program Entry point.
        /// </summary>
        /// <param name="args">Program args.</param>
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        /// <summary>
        ///     Setup web hosting.
        /// </summary>
        /// <param name="args">WebHost args.</param>
        /// <returns>WebHost builder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    var foundPort = int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port);
                    if (foundPort)
                    {
                        webBuilder.UseUrls($"http://*:{port}"); //DevSkim: ignore DS137138
                    }
                });
    }
}