using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WeatherLink.Models;
using WeatherLink.Services;

namespace WeatherLink.Controllers
{
	/// <summary>
	/// Interact with the traffic advice service from Slack.
	/// </summary>
	[Route("api/[controller]")]
	public class SlackController : Controller
	{
		private readonly IDistanceToDurationService _distanceToDurationService;
		private readonly IGeocodeService _geocodeService;
		private readonly IOptions<WeatherLinkSettings> _optionsAccessor;
		private readonly ITrafficAdviceService _trafficAdviceService;
		private readonly SlackWorkspaceAppContext _context;

		//TODO: I don't like this
		private readonly TrafficAdviceController _adviceController;

		/// <summary>
		/// Interact with the traffic advice service from Slack.
		/// </summary>
		public SlackController(IOptions<WeatherLinkSettings> optionsAccessor, ITrafficAdviceService trafficAdviceService, IGeocodeService geocodeService, IDistanceToDurationService distanceToDurationService, SlackWorkspaceAppContext slackWorkspaceAppContext)
		{
			_optionsAccessor = optionsAccessor;
			_trafficAdviceService = trafficAdviceService;
			_geocodeService = geocodeService;
			_distanceToDurationService = distanceToDurationService;
			_context = slackWorkspaceAppContext;
			_adviceController = new TrafficAdviceController(_optionsAccessor, _trafficAdviceService, _geocodeService, _distanceToDurationService);
		}

		/// <summary>
		/// An endpoint for installing the app in Slack.
		/// </summary>
		[Route("install")]
		[HttpGet]
		public ContentResult Install()
		{
			string button = $"<a href=\"https://slack.com/oauth/authorize?scope=incoming-webhook,commands,bot&client_id={_optionsAccessor.Value.SlackClientId}&redirect_uri=https://{HttpContext.Request.Host}/api/slack/authorize\"><img alt=\"Add to Slack\" height=\"40\" width=\"139\" src=\"https://platform.slack-edge.com/img/add_to_slack.png\" srcset=\"https://platform.slack-edge.com/img/add_to_slack.png 1x, https://platform.slack-edge.com/img/add_to_slack@2x.png 2x\" /></a>";
			return new ContentResult
			{
				Content = button,
				ContentType = "text/html"
			};
		}

		/// <summary>
		/// An endpoint for authorizing use in Slack.
		/// </summary>
		[Route("authorize")]
		[HttpGet]
		public async Task<ContentResult> Authorize(string code)
		{
			var formContent = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("client_id", _optionsAccessor.Value.SlackClientId),
				new KeyValuePair<string, string>("client_secret", _optionsAccessor.Value.SlackClientSecret),
				new KeyValuePair<string, string>("code", code),
				new KeyValuePair<string, string>("redirect_uri", $"https://{HttpContext.Request.Host}/api/slack/authorize")
			};

			var httpClient = new HttpClient();
			var request = new HttpRequestMessage(HttpMethod.Post, $"{_optionsAccessor.Value.SlackApiBase}api/oauth.token")
			{
				Content = new FormUrlEncodedContent(formContent)
			};
			var result = await httpClient.SendAsync(request);
			var resultJsonString = await result.Content.ReadAsStringAsync();
			var workspaceTokenResponse = JsonConvert.DeserializeObject<dynamic>(resultJsonString);

			_context.WorkspaceTokens.Add(new WorkspaceToken
			{
				Id = new Guid(),
				AppId = workspaceTokenResponse.app_id,
				TeamId = workspaceTokenResponse.team_id,
				Token = workspaceTokenResponse.access_token
			});
			_context.SaveChanges();

			return new ContentResult
			{
				Content = "All set up!",
				ContentType = "text/html"
			};
		}

		/// <summary>
		/// An endpoint for handling messages from slack.
		/// </summary>
		/// <param name="text">The slack message, it should match "^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$"</param>
		/// <param name="token">The slack token to verify it's a team that is setup in WeatherLinkSettings.SlackTokens.</param>
		/// <returns>A string value describing when to leave based on the weather.</returns>
		[HttpPost]
		public async Task<SlackResponse> SlackIntegration(string text, string token)
		{
			if (token != _optionsAccessor.Value.SlackVerificationToken)
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

				if (string.IsNullOrWhiteSpace(endingLocation) && hasHours)
				{
					advice = await _adviceController.GetTrafficAdviceForATime(startingLocation, hoursFromNow);
				}
				else if (!string.IsNullOrWhiteSpace(endingLocation) && !hasHours)
				{
					advice = await _adviceController.GetTrafficAdviceToALocation(startingLocation, endingLocation);
				}
				else if (!string.IsNullOrWhiteSpace(endingLocation) && hasHours)
				{
					advice = await _adviceController.GetTrafficAdviceToALocationForATime(startingLocation, endingLocation, hoursFromNow);
				}
				else
				{
					advice = await _adviceController.GetTrafficAdvice(startingLocation);
				}
			}

			var message = (advice == null)
				? "An error occurred fetching current data."
				: $"{advice}{Environment.NewLine}<{advice.DataSource}|{advice.AttributionLine}>";

			Response.StatusCode = (int)HttpStatusCode.OK;
			return new SlackResponse { response_type = "in_channel", text = Regex.Replace(message, @"\r\n?|\n", "\n") };
		}
	}
}