// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XForm.NetApps.Builders.WinService.Interfaces;

public interface IWorkerServiceInjector
{
	/// <summary>
	/// Inject any specificservices needed by the worker (during instantiation) here.
	/// </summary>
	/// <param name="serviceCollection"></param>
	/// <param name="workerConfiguration">Only the worker configuration section from the json config (e.g. EmailWorker).</param>
	void ConfigureServicesForWorker(IServiceCollection serviceCollection, IConfiguration workerConfiguration);
}
