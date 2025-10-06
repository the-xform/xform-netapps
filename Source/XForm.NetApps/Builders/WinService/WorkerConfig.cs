// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

namespace XForm.NetApps.Builders.WinService;

/// <summary>
/// Worker configuration needed by the factories to instantiate a worker.
/// </summary>
public class WorkerConfig
{
    /// <summary>
    /// Unique id of the worker.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Name of the worker.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Schedule for the worker.
    /// </summary>
    public required ScheduleConfiguration Schedule { get; init; }

	/// <summary>
	/// Type for the factory that will instantiate the worker.
	/// </summary>
	public required string FactoryType { get; set; }

    /// <summary>
    /// Path of the assembly that contains the worker.
    /// </summary>
    public required string AssemblyPath { get; set; }

	/// <summary>
	/// The type name of the service injector that will be used to inject the services required by this worker.
	/// </summary>
	public required string WorkerServiceInjectorType { get; set; }
}