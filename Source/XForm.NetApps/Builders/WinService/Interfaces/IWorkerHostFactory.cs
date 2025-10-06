// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;

namespace XForm.NetApps.Builders.WinService.Interfaces;

public interface IWorkerHostFactory
{
	/// <summary>
	/// Worker host factory for creating the worker host based on provided configuration.
	/// </summary>
	/// <param name="workerConfiguration">Only a single worker configuration section (object) from the json config file.</param>
	/// <param name="serviceProvider"></param>
	/// <param name="assemblyPath"></param>
	/// <param name="workerFactoryType"></param>
	/// <returns></returns>
	WorkerHost Create(
      IConfiguration workerConfiguration,
      IServiceProvider serviceProvider,
      string assemblyPath,
      string workerFactoryType);
}
