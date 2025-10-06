// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using XForm.NetApps.Providers;
using XForm.Core.Interfaces;
using XForm.Utilities;

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
        // Add all common services here.
        services.AddSingleton<ISequentialGuidProvider, SequentialGuidProvider>();
        services.AddSingleton<IJsonUtilities, JsonUtilities>();

        Logger logger = new LoggerConfiguration().ReadFrom.Configuration(globalConfiguration).CreateLogger();

		services.AddSerilog(logger);

        return services;
    }
}
