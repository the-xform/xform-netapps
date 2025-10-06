// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Reflection;

namespace XForm.Core.Interfaces
{
    public interface IAssemblyLoader
    {
		/// <summary>
		/// Loads the assembly from the specified file name.
		/// </summary>
		/// <param name="assemblyFileName"></param>
		/// <returns></returns>
		Assembly Load(string assemblyFileName);
    }
}
