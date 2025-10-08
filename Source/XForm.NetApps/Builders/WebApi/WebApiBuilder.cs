// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Builders.WebApi;

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
	/// 
	/// </summary>
	/// <param name="webApiOptions"></param>
	/// <returns></returns>
	public static WebApplicationBuilder CreateWebApplicationBuilder(WebApiOptions webApiOptions)
	{
		Xssert.IsNotNull(webApiOptions.ApiName, nameof(webApiOptions.ApiName));

		var host_app_builder = WebApplication.CreateBuilder(webApiOptions.Args);
		host_app_builder.ConfigureApplicationBuilder(new HostApplicationBuilderSettings
		{
			ApplicationName = webApiOptions.ApiName,
			Args = webApiOptions.Args
		});

		return host_app_builder;
	}

	#endregion - Public Methods -
}
