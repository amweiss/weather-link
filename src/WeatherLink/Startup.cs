using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
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
		public void Configure(IApplicationBuilder app)
		{
			app.UseMvc();

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherLink V1");
				c.RoutePrefix = "";
			});

			app.UseStaticFiles();

			// Migrate db
			using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
			{
				scope.ServiceProvider.GetService<SlackWorkspaceAppContext>().Database.Migrate();
			}
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services
			services.AddMvc();
			services.AddOptions();

			// Get config
			services.Configure<WeatherLinkSettings>(Configuration);

			// Setup token db
			services.AddDbContext<SlackWorkspaceAppContext>(options =>
				options.UseSqlServer(
					Configuration.GetValue<string>(
						nameof(WeatherLinkSettings.SlackTokenDbConnection))));

			// Add custom services
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
				c.IncludeXmlComments("wwwroot/WeatherLink.xml");
				c.DescribeAllEnumsAsStrings();
			}
			);
		}
	}
}