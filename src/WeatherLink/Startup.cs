using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSwag.AspNetCore;
using WeatherLink.Models;
using WeatherLink.Services;
using NSwag;

namespace WeatherLink
{
	internal class Startup
	{
		public Startup(IWebHostEnvironment env)
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

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services
			services.AddMvc().AddNewtonsoftJson(); //TODO: Change this to AddControllers when possible
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
				c.PostProcess = (document) => document.Schemes = new[] { OpenApiSchema.Https };
			});
		}
	}
}