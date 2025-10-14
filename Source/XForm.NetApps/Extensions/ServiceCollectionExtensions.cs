// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using XForm.Core.Interfaces;
using XForm.NetApps.ConfigurationSettings;
using XForm.NetApps.Interfaces;
using XForm.NetApps.Providers;
using XForm.Utilities;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds all the core common services that are expected to be required by all the host applications (web, windows, services, apis etc.).
	/// </summary>
	/// <param name="services"></param>
	/// <returns></returns>
	public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration globalConfiguration)
	{
		// Inject logger
		var logger = new LoggerConfiguration().ReadFrom.Configuration(globalConfiguration).CreateLogger();
		services.AddSerilog(logger);

		// Add all common services here.
		services.AddSingleton<ISequentialGuidProvider, SequentialGuidProvider>();
		services.AddSingleton<IJsonUtilities, JsonUtilities>();
		services.AddSingleton<IConfigProxyProvider, ConfigProxyProvider>();
		services.AddSingleton<ICertificateProvider, CertificateProvider>();

		// Inject SqlDbContextProvider if configured.
		var sql_connections_config_section = globalConfiguration.GetSection("SqlConnectionSettings");
		if (sql_connections_config_section != null)
		{
			// Inject IDbContext for each of the connectionstrings provided in configuration.
			var sql_connection_strings_settings = sql_connections_config_section.Get<SqlConnectionSettings>();
			if (sql_connection_strings_settings != null
				&& sql_connection_strings_settings.IsEnabled == true)
			{
				Xssert.IsNotNull(sql_connection_strings_settings.ConnectionStrings);

				foreach (KeyValuePair<string, string> connectionStringPair in sql_connection_strings_settings.ConnectionStrings)
				{
					Xssert.IsNotNullOrEmpty(connectionStringPair.Value);
					services.AddKeyedSingleton<IDbContextProvider>($"{connectionStringPair.Key}", (sp, key) => new SqlDbContextProvider(connectionStringPair.Key, connectionStringPair.Value));
				}
			}
		}

		return services;
	}
}
