// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;

namespace XForm.NetApps.Builders.WinService.Interfaces;

public interface IWorkerFactory
{
    /// <summary>
    /// Factory for creating the background worker based on provided configuration.
    /// </summary>
    /// <param name="workerConfiguration">Only a single worker configuration section (object) from configuration. E.g. EmailWorker.</param>
    /// <returns></returns>
    IWorker Create(IConfiguration workerConfiguration, IServiceProvider serviceProvider);
}
