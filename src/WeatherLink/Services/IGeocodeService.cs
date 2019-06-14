// Copyright (c) Adam Weiss. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WeatherLink.Services
{
	using System;
	using System.Threading.Tasks;

	/// <summary>
	/// A service to convert an address to a latitude and longitude.
	/// </summary>
	public interface IGeocodeService
	{
		/// <summary>
		/// Convert address to a latitude and longitude Tuple.
		/// </summary>
		/// <param name="address">The string to attempt to convert into a latitude and longitude.</param>
		/// <returns>The Tuple of latitude and longitude in degrees.</returns>
		Task<Tuple<double, double>> Geocode(string address);
	}
}