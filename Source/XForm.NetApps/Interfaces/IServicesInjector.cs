// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XForm.Core.Interfaces;

/// <summary>
/// Service injector interface to inject any app-specific configuration and services into the application.
/// </summary>
public interface IServicesInjector
{
	/// <summary>
	/// Inject any additional app-specific configuration here.
	/// </summary>
	/// <param name="configurationBuilder"></param>
	void ConfigureConfiguration(IConfigurationBuilder configurationBuilder);

	/// <summary>
	/// Inject any services used by the application here.
	/// </summary>
	/// <param name="configuration">Global configuration object.</param>
	/// <param name="serviceCollection"></param>
	void ConfigureServices(IConfiguration configuration, IServiceCollection serviceCollection);
}
