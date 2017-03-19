using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
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
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true) //TODO: Find better way to handle hosting options and debug paths
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
				.AddJsonFile("src/WeatherLink/appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"src/WeatherLink/appsettings.{env.EnvironmentName}.json", optional: true)
				.AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfigurationRoot Configuration { get; }

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			app.UseStaticFiles();

			app.UseMvc();

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherLink V1");
			});
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services
			services.AddMvc();
			services.AddOptions();

			// Add custom services
			services.Configure<WeatherLinkSettings>(Configuration);
			services.AddTransient<ITrafficAdviceService, WeatherBasedTrafficAdviceService>();
			services.AddTransient<IGeocodeService, GoogleMapsGeocodeService>();
			services.AddTransient<IDistanceToDurationService, GoogleMapsDistanceToDurationService>();
			services.AddTransient<IDarkSkyService, HourlyAndMinutelyDarkSkyService>();

			// Configure swagger
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info
				{
					Title = "WeatherLink",
					Description = "An API to get weather based advice.",
				});
				//c.IncludeXmlComments(GetXmlCommentsPath());
				c.DescribeAllEnumsAsStrings();
			}
			);
		}

		private string GetXmlCommentsPath()
		{
			var app = PlatformServices.Default.Application;
			return Path.Combine(app.ApplicationBasePath, Path.ChangeExtension(app.ApplicationName, "xml"));
		}
	}
}