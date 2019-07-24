// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

#region

using System.Diagnostics.CodeAnalysis;

#endregion

[assembly:
    SuppressMessage("StyleCop.CSharp.DocumentationRules",
        "SA1642:Constructor summary documentation should begin with standard text",
        Justification = "Standard text isn't helpful", Scope = "namespaceanddescendants", Target = "WeatherLink")]
[assembly:
    SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this",
        Justification = "Verbose", Scope = "namespaceanddescendants", Target = "WeatherLink")]
[assembly:
    SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task",
        Justification = "Not needed for core app", Scope = "namespaceanddescendants", Target = "WeatherLink")]
[assembly:
    SuppressMessage("Style", "IDE0017:Simplify object initialization",
        Justification = "Analyzers can't detect this is ok when simplified", Scope = "member",
        Target =
            "~M:WeatherLink.Controllers.SlackController.Authorize(System.String)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Mvc.ContentResult}")]
[assembly:
    SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters",
        Justification = "Not globalizing", Scope = "namespaceanddescendants", Target = "WeatherLink")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1812:Avoid uninstantiated internal classes",
        Justification = "Handled with DI", Scope = "type", Target = "~T:WeatherLink.Startup")]
[assembly:
    SuppressMessage("Microsoft.Performance", "CA1812:Avoid uninstantiated internal classes",
        Justification = "Handled with DI", Scope = "type",
        Target = "~T:WeatherLink.Services.WeatherBasedTrafficAdviceService")]
[assembly:
    SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "We want to return a message either way.", Scope = "member",
        Target =
            "~M:WeatherLink.Controllers.SlackController.SlackIntegration(System.String)~System.Threading.Tasks.Task{Microsoft.AspNetCore.Mvc.ActionResult{WeatherLink.Models.SlackResponse}}")]
[assembly:
    SuppressMessage("Microsoft.Reliability",
        "CA2007:Consider calling ConfigureAwait on the awaited task",
        Justification = "Don't need this for our app", Scope = "namespaceanddescendants", Target = "WeatherLink")]