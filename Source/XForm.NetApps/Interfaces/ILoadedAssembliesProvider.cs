// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Reflection;

namespace XForm.Core.Interfaces;

/// <summary>
/// Provides all the currently loaded assemblies in the application domain.
/// </summary>
public interface ILoadedAssembliesProvider
{
	/// <summary>
	/// Gets all the currently loaded assemblies in the application domain.
	/// </summary>
	/// <returns></returns>
	IEnumerable<Assembly> GetAll();
}
