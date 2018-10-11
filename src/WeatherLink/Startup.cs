using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSwag.AspNetCore;
using WeatherLink.Models;
using WeatherLink.Services;

namespace WeatherLink
{
	internal class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
			app.UseMvc();

			app.UseSwaggerUi3WithApiExplorer(c =>
			{
				c.SwaggerRoute = "/swagger/v1/swagger.json";
                c.SwaggerUiRoute = "/swagger";
				c.GeneratorSettings.Title = "WeatherLink";
				c.GeneratorSettings.Description = "An API to get weather based advice.";
			});

			app.UseStaticFiles();
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
			services.AddSwagger();
		}
	}
}