// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Hosting;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Builders.Console;

/// <summary>
/// Creates and returns a HostApplicationBuilder for a Console application.
/// </summary>
public static class ConsoleAppBuilder
{
	#region - Public Methods -

	/// <summary>
	/// Builds and returns a HostApplicationBuilder for a Console application.
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <param name="appOptions"></param>
	/// <returns></returns>
	public static HostApplicationBuilder CreateHostBuilder(ConsoleAppOptions appOptions)
	{
		Xssert.IsNotNull(appOptions.AppName, nameof(appOptions.AppName));

		var host_app_builder = CommonAppBuilder.CreateCommonApplicationBuilder(appOptions.AppName, appOptions.Args);

		// Register assembly providers for assembly resolution.
		CommonAppBuilder.RegisterAssemblyProviders(host_app_builder);

		return (HostApplicationBuilder)host_app_builder;
	}

	/// <summary>
	/// Builds and returns IHost for a Console application.
	/// </summary>
	/// <param name="hostBuilder"></param>
	/// <param name="appOptions"></param>
	/// <returns></returns>
	public static IHost CreateHost(ConsoleAppOptions appOptions)
	{
		return CreateHostBuilder(appOptions).Build();
	}

	#endregion - Public Methods -
}
