// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Hosting;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Builders.Console;

/// <summary>
/// Creates and returns a HostApplicationBuilder for a Console application with all injected common services..
/// </summary>
public static class ConsoleAppBuilder
{
	#region - Public Methods -

	/// <summary>
	/// Builds and returns a HostApplicationBuilder for a Console application with all injected common services.
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <param name="appOptions"></param>
	/// <returns></returns>
	public static HostApplicationBuilder CreateAppHostBuilder(ConsoleAppOptions appOptions)
	{
		Xssert.IsNotNull(appOptions.AppName, nameof(appOptions.AppName));

		var host_app_builder = CommonAppBuilder.CreateHostApplicationBuilder(appOptions.AppName, appOptions.Args);

		return host_app_builder;
	}

	/// <summary>
	/// Builds and returns a IHostBuilder with all injected common services.
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <param name="appOptions"></param>
	/// <returns></returns>
	public static IHostBuilder CreateHostBuilder(ConsoleAppOptions appOptions)
	{
		Xssert.IsNotNull(appOptions.AppName, nameof(appOptions.AppName));

		var host_builder = CommonAppBuilder.CreateHostBuilder(appOptions.AppName, appOptions.Args);

		return host_builder;
	}

	#endregion - Public Methods -
}
