// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace XForm.Core.Interfaces;

/// <summary>
/// Logger configuration extensions to add Serilog as the logging provider.
/// </summary>
public static class LoggerConfigurationExtensions
{
	/// <summary>
	/// Adds Serilog as the logging provider with default configuration read from appsettings.json or other configuration source.
	/// </summary>
	/// <param name="serviceCollection"></param>
	/// <param name="configuration"></param>
	/// <returns></returns>
	public static IServiceCollection AddSerilog(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        return serviceCollection.AddLogging(loggingBuilder =>
            {
                serviceCollection.AddSerilog(GetDefaultLogger(configuration));
            });
    }

	/// <summary>
	/// Adds Serilog as the logging provider with the provided logger instance.
	/// </summary>
	/// <param name="serviceCollection"></param>
	/// <param name="logger"></param>
	/// <returns></returns>
	public static IServiceCollection AddSerilog(this IServiceCollection serviceCollection, Serilog.ILogger logger)
    {
        return serviceCollection.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(logger);
            });
    }

	/// <summary>
	/// Gets the default Serilog logger instance with configuration read from the provided configuration source.
	/// </summary>
	/// <param name="configuration"></param>
	/// <returns></returns>
	public static Serilog.ILogger GetDefaultLogger(IConfiguration configuration)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .CreateLogger();
        
        return logger;
    }
}
