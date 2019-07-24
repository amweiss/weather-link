// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

#region

using System.Diagnostics.CodeAnalysis;

#endregion

[assembly:
    SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this",
        Justification = "Verbose", Scope = "namespaceanddescendants", Target = "WeatherLink.UnitTests")]
[assembly:
    SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented",
        Justification = "Tests are well named", Scope = "namespaceanddescendants", Target = "WeatherLink.UnitTests")]