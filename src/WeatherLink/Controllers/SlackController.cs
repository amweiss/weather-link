// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
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

        // TODO: I don't like this
        private readonly TrafficAdviceController adviceController;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackController"/> class.
        /// </summary>
        /// <param name="optionsAccessor">Service to access options from startup.</param>
        /// <param name="trafficAdviceService">Service to get traffic advice.</param>
        /// <param name="geocodeService">Service to turn text into a geolocation.</param>
        /// <param name="distanceToDurationService">Service to convert a distance to a duration based on traffic.</param>
        public SlackController(
            IOptions<WeatherLinkSettings> optionsAccessor,
            ITrafficAdviceService trafficAdviceService,
            IGeocodeService geocodeService,
            IDistanceToDurationService distanceToDurationService)
        {
            this.optionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));
            this.trafficAdviceService = trafficAdviceService ?? throw new ArgumentNullException(nameof(trafficAdviceService));
            this.geocodeService = geocodeService ?? throw new ArgumentNullException(nameof(geocodeService));
            this.distanceToDurationService = distanceToDurationService ?? throw new ArgumentNullException(nameof(distanceToDurationService));
            adviceController = new TrafficAdviceController(this.trafficAdviceService, this.geocodeService, this.distanceToDurationService);
        }

        /// <summary>
        /// An endpoint for handling messages from slack.
        /// </summary>
        /// <param name="text">The slack message, it should match "^(?:in (\d*[.,]?\d*) hours? from )?(.+?)(?: for (.+))?$".</param>
        /// <returns>A string value describing when to leave based on the weather.</returns>
        [HttpPost]
        public async Task<SlackResponse> SlackIntegration(string text)
        {
            if (!VerifySlackSignature())
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

                var hasHours = double.TryParse(hours, out var hoursFromNow);

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

        private bool VerifySlackSignature()
        {
            var signatureMatch = false;
            // Make sure request was recent to prevent replay attack.
            if (int.TryParse(Request.Headers["X-Slack-Request-Timestamp"], out var timestamp)
                && Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp) <= 60 * 5)
            {
                var signature = $"v0:{timestamp}:{Request.Body.ToString()}";
                var encoding = new ASCIIEncoding();
                using var hmac = new HMACSHA256(encoding.GetBytes(optionsAccessor.Value.SlackSigningSecret));
                var hashedSignature = hmac.ComputeHash(encoding.GetBytes(signature));
                var mySignature = $"v0={encoding.GetString(hashedSignature)}";

                Console.WriteLine($"mySignature: {mySignature}");
                Console.WriteLine($"slackSignature: {Request.Headers?["X-Slack-Signature"]}");

                signatureMatch = encoding.GetBytes(mySignature) == encoding.GetBytes(Request.Headers?["X-Slack-Signature"]);
            }
            return signatureMatch;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            adviceController.Dispose();
        }
    }
}