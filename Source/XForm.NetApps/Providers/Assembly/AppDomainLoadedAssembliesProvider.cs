// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using XForm.Core.Interfaces;

namespace XForm.NetApps.Providers.Assembly;

/// <summary>
/// Provides all the asemblies from the current app domain.
/// </summary>
public class AppDomainLoadedAssembliesProvider : ILoadedAssembliesProvider
{
    /// <summary>
    /// Get all the asemblies from the current app domain.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<System.Reflection.Assembly> GetAll()
    {
        return AppDomain.CurrentDomain.GetAssemblies();
    }
}
