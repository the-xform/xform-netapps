// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XForm.NetApps.Builders;

public class ServiceOptions
{
    public virtual Action<IConfigurationBuilder, IServiceCollection> ApplicationSetup { get; init; } = (c, s) => { };
}
