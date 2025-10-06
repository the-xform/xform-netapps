// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Reflection;

namespace XForm.Core.Interfaces;

/// <summary>
/// To loads a given assembly.
/// </summary>
public interface IAssemblyProvider
{
    /// <summary>
    /// Gets the given assembly name.
    /// </summary>
    /// <param name="assemblyName"></param>
    /// <returns></returns>
    Assembly? Get(AssemblyName assemblyName); 
}