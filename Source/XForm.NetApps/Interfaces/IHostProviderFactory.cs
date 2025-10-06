// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace XForm.Core.Interfaces;

/// <summary>
/// Host provider factory.
/// </summary>
public interface IHostProviderFactory
{
    IHostProvider Create(string[] args);
}

/// <summary>
/// Host provider.
/// </summary>
public interface IHostProvider
{
    IConfigurationBuilder ConfigurationBuilder { get; }

    IConfiguration Configuration { get; }

    IServiceCollection Services { get; }

    IHostEnvironment Environment { get; }

    IHost Host { get; }

    IConfiguration HostConfiguration { get; }

    void Build();
}
