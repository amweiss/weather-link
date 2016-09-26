using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.Swagger.Model;
using System.IO;
using WeatherLink.Models;
using WeatherLink.Services;

namespace WeatherLink {

    class Startup {

        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Add framework services
            services.AddMvc();
            services.AddOptions();

            // Add custom services
            services.Configure<WeatherLinkSettings>(Configuration);
            services.AddTransient<ITrafficAdviceService, WeatherBasedTrafficAdviceService>();
            services.AddTransient<IGeocodeService, GoogleMapsGeocodeService>();
            services.AddTransient<IDistanceToDurationService, GoogleMapsDistanceToDurationService>();

            // Configure swagger
            services.AddSwaggerGen();
            services.ConfigureSwaggerGen(options => {
                options.SingleApiVersion(new Info {
                    Version = "v1",
                    Title = "Weather Link",
                    Description = "An API to get weather based advice.",
                    TermsOfService = "None"
                });
                options.IncludeXmlComments(GetXmlCommentsPath());
                options.DescribeAllEnumsAsStrings();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseStaticFiles();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
        }

        string GetXmlCommentsPath() {
            var app = PlatformServices.Default.Application;
            return Path.Combine(app.ApplicationBasePath, Path.ChangeExtension(app.ApplicationName, "xml"));
        }
    }
}