// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using WeatherLink.Models;
    using WeatherLink.Services;

    /// <summary>
    /// Start up configuration class.
    /// </summary>
    internal class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="env">Hosting environment.</param>
        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the configuration object.
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Builder object for the running application.</param>
        /// <param name="env">Hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });

            app.UseOpenApi();
            app.UseSwaggerUi3();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The service injector.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services
            services.AddControllers();
            services.AddHealthChecks();
            services.AddOptions();

            // Get config
            services.Configure<WeatherLinkSettings>(Configuration);

            // Setup token db
            services.AddDbContext<SlackWorkspaceAppContext>();

            // Add custom services
            services.AddTransient<ITrafficAdviceService, WeatherBasedTrafficAdviceService>();
            services.AddTransient<IGeocodeService, GoogleMapsGeocodeService>();
            services.AddTransient<IDistanceToDurationService, GoogleMapsDistanceToDurationService>();
            services.AddTransient<IDarkSkyService, HourlyAndMinutelyDarkSkyService>();

            // Configure swagger
            services.AddOpenApiDocument(c =>
            {
                c.Title = "WeatherLink";
                c.Description = "An API to get weather based advice.";
            });
        }
    }
}