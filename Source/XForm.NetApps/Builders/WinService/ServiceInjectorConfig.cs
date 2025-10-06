// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

namespace XForm.NetApps.Builders.WinService;

/// <summary>
/// The configuration needed to load a service injector that will inject services needed by a worker.
/// </summary>
public class ServiceInjectorConfig
{
	/// <summary>
	/// Is the service injector enabled?
	/// </summary>
	public required bool IsEnabled { get; set; }

	/// <summary>
	/// Path of the assembly that contains the service injector.
	/// </summary>
	public required string AssemblyPath { get; set; }

	/// <summary>
	/// Name of the type that implements IServiceInjector interface and will be used to inject services.
	/// </summary>
	public required string TypeName { get; set; }
}
