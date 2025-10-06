// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
	public static WebApplicationBuilder CreateHostBuilder(WebApiOptions webApiOptions)
	{
		Xssert.IsNotNull(webApiOptions.ApiName, nameof(webApiOptions.ApiName));

		var host_app_builder = CommonAppBuilder.CreateCommonApplicationBuilder(webApiOptions.ApiName, webApiOptions.Args);

		// Register assembly providers for assembly resolution.
		CommonAppBuilder.RegisterAssemblyProviders(host_app_builder);

		var weapi_builder = (WebApplicationBuilder)host_app_builder;

		weapi_builder.Host.UseContentRoot(AppContext.BaseDirectory);
		weapi_builder.Services.AddControllers();
		weapi_builder.Services.AddEndpointsApiExplorer();


		return (WebApplicationBuilder)host_app_builder;
	}

	/// <summary>
	/// Builds and returns IHost for a WebApi application.
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <param name="webApiOptions"></param>
	/// <returns></returns>
	public static IHost CreateHost(WebApiOptions webApiOptions)
	{
		return CreateHostBuilder(webApiOptions).Build();
	}

	#endregion - Public Methods -
}
