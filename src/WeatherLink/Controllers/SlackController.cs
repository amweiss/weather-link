#region

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeatherLink.Models;
using WeatherLink.Services;

#endregion

namespace WeatherLink.Controllers
{
    /// <summary>
    ///     Interact with the traffic advice service from Slack.
    /// </summary>
    [Route("api/[controller]")]
    public class SlackController : Controller
    {
        // TODO: I don't like this
        private readonly TrafficAdviceController adviceController;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SlackController" /> class.
        /// </summary>
        /// <param name="trafficAdviceService">Service to get traffic advice.</param>
        /// <param name="geocodeService">Service to turn text into a geolocation.</param>
        /// <param name="distanceToDurationService">Service to convert a distance to a duration based on traffic.</param>
        public SlackController(
            //IOptions<WeatherLinkSettings> optionsAccessor,
            ITrafficAdviceService trafficAdviceService,
            IGeocodeService geocodeService,
            IDistanceToDurationService distanceToDurationService)
        {
            adviceController = new TrafficAdviceController(trafficAdviceService, geocodeService, distanceToDurationService);
        }

        /// <summary>
        ///     An endpoint for handling messages from slack.
        /// </summary>
        /// <param name="text">The slack message, it should match "^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$".</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [HttpPost]
        public async Task<ActionResult<SlackResponse>> SlackIntegration(string text)
        {
            if (!VerifySlackSignature())
            {
                return Unauthorized();
            }

            ActionResult<WeatherBasedTrafficAdvice> advice = (WeatherBasedTrafficAdvice)null;

            var checkCommand = Regex.Match(text, @"^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$");

            if (checkCommand.Success)
            {
                var hours = checkCommand.Groups[1].Value;
                var startingLocation = checkCommand.Groups[2].Value;
                var endingLocation = checkCommand.Groups[3].Value;

                var hasHours = double.TryParse(hours, out var hoursFromNow);

                if (hasHours && hoursFromNow < 0)
                {
                    return BadRequest();
                }

                try
                {
                    if (string.IsNullOrWhiteSpace(endingLocation) && hasHours)
                    {
                        advice = await adviceController.GetTrafficAdviceForATime(startingLocation, hoursFromNow).ConfigureAwait(false);
                    }
                    else if (!string.IsNullOrWhiteSpace(endingLocation) && !hasHours)
                    {
                        advice = await adviceController.GetTrafficAdviceToALocation(startingLocation, endingLocation).ConfigureAwait(false);
                    }
                    else if (!string.IsNullOrWhiteSpace(endingLocation) && hasHours)
                    {
                        advice = await adviceController.GetTrafficAdviceToALocationForATime(startingLocation,
                            endingLocation, hoursFromNow).ConfigureAwait(false);
                    }
                    else
                    {
                        advice = await adviceController.GetTrafficAdvice(startingLocation).ConfigureAwait(false);
                    }
                }
                catch (Exception)
                {
                    advice = null;
                }
            }

            var message = advice == null
                ? "An error occurred fetching current data."
                : $"{advice.Value}{Environment.NewLine}<{advice.Value.DataSource}|{advice.Value.AttributionLine}>";
            
            return new SlackResponse {ResponseType = "in_channel", Text = Regex.Replace(message, @"\r\n?|\n", "\n")};
        }

        //TODO: actual checking doesn't work for some reason.
        // Make sure request was recent to prevent replay attack.
        private bool VerifySlackSignature() =>
            int.TryParse(Request.Headers["X-Slack-Request-Timestamp"], out var timestamp1)
            && Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp1) <= 60 * 5;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            adviceController.Dispose();
        }
    }
}