// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.Hosting;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Builders.WinForms;

/// <summary>
/// Creates and returns a HostApplicationBuilder for a WebApi application.
/// </summary>
public static class WebApiBuilder
{
	#region - Public Methods -

	/// <summary>
	/// Builds and returns a HostApplicationBuilder for a WebApi application.
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <param name="webApiOptions"></param>
	/// <returns></returns>
	public static IHostBuilder CreateHostBuilder(WebApiOptions webApiOptions)
	{
		Xssert.IsNotNull(webApiOptions.ApiName, nameof(webApiOptions.ApiName));

		var host_builder = CommonAppBuilder.CreateHostBuilder(webApiOptions.ApiName, webApiOptions.Args);

		return host_builder;
	}

	/// <summary>
	/// Builds and returns IHost for a WebApi application.
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <param name="webApiOptions"></param>
	/// <returns></returns>
	public static IHost CreateHost(WebApiOptions webApiOptions)
	{
		Xssert.IsNotNull(webApiOptions.ApiName, nameof(webApiOptions.ApiName));

		var host_builder = CreateHostBuilder(webApiOptions).Build();

		return host_builder;
	}

	#endregion - Public Methods -
}
