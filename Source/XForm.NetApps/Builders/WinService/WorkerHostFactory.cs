// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XForm.NetApps.Builders.WinService.Interfaces;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Builders.WinService;

public class WorkerHostFactory: IWorkerHostFactory
{
    private readonly ILogger<WorkerHost> _logger;

    public WorkerHostFactory(ILogger<WorkerHost> logger)
    {
        _logger = logger;
    }

    public WorkerHost Create(
      IConfiguration workerConfiguration,
      IServiceProvider serviceProvider,
      string assemblyPath,
      string workerFactoryType)
    {
        var worker_config = workerConfiguration.Get<WorkerConfig>();
		Xssert.IsNotNull(worker_config, nameof(worker_config));

        // Create worker factory
        var worker_factory_for_host = new WorkerFactoryForWorkerHost(serviceProvider);

        // Create the worker
        _logger.LogInformation($"Creating worker from worker factory of type '{workerFactoryType}'.");
        var worker = worker_factory_for_host.Create(workerConfiguration, assemblyPath, workerFactoryType);

        // Load the worker host along with the worker.
        _logger.LogInformation($"Registering the worker of type '{worker.Name}' with worker host.");
        return new WorkerHost(_logger, worker, Schedule.Build(worker_config.Schedule));
	}

    // FUTURE: Explore using the assemblyQualifiedTypeName instead of assemblyPath
    //public WorkerHost Create(
    //  IConfiguration workerConfiguration, string assemblyQualifiedTypeName)
    //{
    //    var worker_config = workerConfiguration.Get<WorkerConfig>();
    //    // TODO
    //    //ConfigUtils.AssertIsNotNull<WorkerConfig>(workerConfig, "worker_config");

    //    var worker_factory = new WorkerForWorkerHostFactory(_typeLoader);
    //    var worker = worker_factory.Create(workerConfiguration, assemblyQualifiedTypeName);
    //    var schedule = ScheduleUtilities.BuildSchedule(worker_config.Schedule);

    //    return new WorkerHost(_logger, worker, schedule);
    //}
}


