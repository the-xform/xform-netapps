// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XForm.NetApps.Builders.WinService.Interfaces;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Builders.WinService;

public class WorkerFactoryForWorkerHost
{
	// FUTURE: Consider using TypeLoader way
	//private readonly ITypeLoader _typeLoader;

	//public WorkerForWorkerHostFactory(ITypeLoader typeLoader)
	//{
	//    _typeLoader = typeLoader;
	//}

	//public IWorker Create(IConfiguration workerConfiguration, string assemblyQualifiedTypeName)
	//{
	//    var worker_config = workerConfiguration.Get<WorkerConfig>();
	//    // TODO: Just ensure worker_config is not null and throw error if it is.

	//    var factory = _typeLoader.InstantiateType<IWorkerFactory>(assemblyQualifiedTypeName);
	//    return factory.Create(workerConfiguration);
	//}

	private readonly IServiceProvider _serviceProvider;

	public WorkerFactoryForWorkerHost(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

	/// <summary>
	/// Instantiates the configured worker factory and the worker from that factory.
	/// </summary>
	/// <param name="workerConfiguration">Only a single worker configuration section (object) from configuration. E.g. EmailWorker.</param>
	/// <param name="assemblyPath">Path to the assembly file (.dll) for the worker.</param>
	/// <param name="workerSpecificFactoryType">Type of factory that will create the worker.</param>
	/// <returns></returns>
	public IWorker Create(
	  IConfiguration workerConfiguration,
	  string assemblyPath,
	  string workerSpecificFactoryType)
	{
		Xssert.IsNotNull(workerConfiguration.Get<WorkerConfig>(), nameof(workerConfiguration));

		var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
		Xssert.IsNotNull(assembly, nameof(assembly));

		var worker_factory_type = assembly.GetType(workerSpecificFactoryType);
		Xssert.IsNotNull(worker_factory_type, nameof(worker_factory_type));

		// FUTURE: Use TypeLoaderHelper here.
		var worker_factory = ActivatorUtilities.CreateInstance(_serviceProvider, worker_factory_type) as IWorkerFactory;
		Xssert.IsNotNull(worker_factory, nameof(worker_factory));

		// Create worker using the worker factory.
		return worker_factory.Create(workerConfiguration, _serviceProvider);
	}
}
