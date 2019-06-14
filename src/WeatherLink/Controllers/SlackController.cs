// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.Controllers
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Options;
	using Newtonsoft.Json;
	using WeatherLink.Models;
	using WeatherLink.Services;

	/// <summary>
	/// Interact with the traffic advice service from Slack.
	/// </summary>
	[Route("api/[controller]")]
	public class SlackController : Controller
	{
		private readonly IDistanceToDurationService distanceToDurationService;
		private readonly IGeocodeService geocodeService;
		private readonly IOptions<WeatherLinkSettings> optionsAccessor;
		private readonly ITrafficAdviceService trafficAdviceService;
		private readonly SlackWorkspaceAppContext context;

		// TODO: I don't like this
		private readonly TrafficAdviceController adviceController;

		/// <summary>
		/// Initializes a new instance of the <see cref="SlackController"/> class.
		/// </summary>
		/// <param name="optionsAccessor">Service to access options from startup.</param>
		/// <param name="trafficAdviceService">Service to get traffic advice.</param>
		/// <param name="geocodeService">Service to turn text into a geolocation.</param>
		/// <param name="distanceToDurationService">Service to convert a distance to a duration based on traffic.</param>
		/// <param name="slackWorkspaceAppContext">Context to database of slack workspace apps.</param>
		public SlackController(
			IOptions<WeatherLinkSettings> optionsAccessor,
			ITrafficAdviceService trafficAdviceService,
			IGeocodeService geocodeService,
			IDistanceToDurationService distanceToDurationService,
			SlackWorkspaceAppContext slackWorkspaceAppContext)
		{
			this.optionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));
			this.trafficAdviceService = trafficAdviceService ?? throw new ArgumentNullException(nameof(trafficAdviceService));
			this.geocodeService = geocodeService ?? throw new ArgumentNullException(nameof(geocodeService));
			this.distanceToDurationService = distanceToDurationService ?? throw new ArgumentNullException(nameof(distanceToDurationService));
			context = slackWorkspaceAppContext ?? throw new ArgumentNullException(nameof(slackWorkspaceAppContext));
			adviceController = new TrafficAdviceController(this.trafficAdviceService, this.geocodeService, this.distanceToDurationService);
		}

		/// <summary>
		/// An endpoint for installing the app in Slack.
		/// </summary>
		/// <returns>Slack intall button HTML.</returns>
		[Route("install")]
		[HttpGet]
		public ContentResult Install()
		{
			string button = $"<a href=\"https://slack.com/oauth/authorize?scope=incoming-webhook,commands,bot&client_id={optionsAccessor.Value.SlackClientId}&redirect_uri=https://{HttpContext.Request.Host}/api/slack/authorize\"><img alt=\"Add to Slack\" height=\"40\" width=\"139\" src=\"https://platform.slack-edge.com/img/add_to_slack.png\" srcset=\"https://platform.slack-edge.com/img/add_to_slack.png 1x, https://platform.slack-edge.com/img/add_to_slack@2x.png 2x\" /></a>";
			return new ContentResult
			{
				Content = button,
				ContentType = "text/html",
			};
		}

		/// <summary>
		/// An endpoint for authorizing use in Slack.
		/// </summary>
		/// <param name="code">The code sent from Slack for the authorization check.</param>
		/// <returns>A message that authorization has worked.</returns>
		[Route("authorize")]
		[HttpGet]
		public async Task<ContentResult> Authorize(string code)
		{
			if (code is null)
			{
				throw new ArgumentNullException(nameof(code));
			}

			var formContent = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("client_id", optionsAccessor.Value.SlackClientId),
				new KeyValuePair<string, string>("client_secret", optionsAccessor.Value.SlackClientSecret),
				new KeyValuePair<string, string>("code", code),
				new KeyValuePair<string, string>("redirect_uri", $"https://{HttpContext.Request.Host}/api/slack/authorize"),
			};

			using var httpClient = new HttpClient();
			using var request = new HttpRequestMessage(HttpMethod.Post, $"{optionsAccessor.Value.SlackApiBase}api/oauth.token");
			request.Content = new FormUrlEncodedContent(formContent);

			using var result = await httpClient.SendAsync(request);
			var resultJsonString = await result.Content.ReadAsStringAsync();
			var workspaceTokenResponse = JsonConvert.DeserializeObject<dynamic>(resultJsonString);

			context.Database.EnsureCreated();
			context.WorkspaceTokens.Add(new WorkspaceToken
			{
				Id = default,
				AppId = workspaceTokenResponse.app_id,
				TeamId = workspaceTokenResponse.team_id,
				Token = workspaceTokenResponse.access_token,
			});
			context.SaveChanges();

			return new ContentResult
			{
				Content = "All set up!",
				ContentType = "text/html",
			};
		}

		/// <summary>
		/// An endpoint for handling messages from slack.
		/// </summary>
		/// <param name="text">The slack message, it should match "^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$".</param>
		/// <param name="token">The slack token to verify it's a team that is setup in WeatherLinkSettings.SlackTokens.</param>
		/// <returns>A string value describing when to leave based on the weather.</returns>
		[HttpPost]
		public async Task<SlackResponse> SlackIntegration(string text, string token)
		{
			if (token != optionsAccessor.Value.SlackVerificationToken)
			{
				Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				return null;
			}

			var advice = (WeatherBasedTrafficAdvice)null;

			var checkCommand = Regex.Match(text, @"^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$");

			if (checkCommand.Success)
			{
				var hours = checkCommand.Groups?[1]?.Value;
				var startingLocation = checkCommand.Groups?[2]?.Value;
				var endingLocation = checkCommand.Groups?[3]?.Value;

				var hasHours = double.TryParse(hours, out double hoursFromNow);

				if ((hasHours && hoursFromNow < 0) || startingLocation == null)
				{
					Response.StatusCode = (int)HttpStatusCode.BadRequest;
					return null;
				}

				try
				{
					if (string.IsNullOrWhiteSpace(endingLocation) && hasHours)
					{
						advice = await adviceController.GetTrafficAdviceForATime(startingLocation, hoursFromNow);
					}
					else if (!string.IsNullOrWhiteSpace(endingLocation) && !hasHours)
					{
						advice = await adviceController.GetTrafficAdviceToALocation(startingLocation, endingLocation);
					}
					else if (!string.IsNullOrWhiteSpace(endingLocation) && hasHours)
					{
						advice = await adviceController.GetTrafficAdviceToALocationForATime(startingLocation, endingLocation, hoursFromNow);
					}
					else
					{
						advice = await adviceController.GetTrafficAdvice(startingLocation);
					}
				}
				catch (Exception)
				{
					advice = null;
				}
			}

			var message = (advice == null)
				? "An error occurred fetching current data."
				: $"{advice}{Environment.NewLine}<{advice.DataSource}|{advice.AttributionLine}>";

			Response.StatusCode = (int)HttpStatusCode.OK;
			return new SlackResponse { ResponseType = "in_channel", Text = Regex.Replace(message, @"\r\n?|\n", "\n") };
		}

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			adviceController.Dispose();
		}
	}
}