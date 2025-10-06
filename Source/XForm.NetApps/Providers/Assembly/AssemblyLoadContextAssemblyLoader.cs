// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Runtime.Loader;
using Microsoft.Extensions.FileProviders;
using XForm.Core.Interfaces;

namespace XForm.NetApps.Providers.Assembly;

/// <summary>
/// Loads assemblies from a given context (via FileProvider).
/// </summary>
public class AssemblyLoadContextAssemblyLoader : IAssemblyLoader
{
    private readonly IFileProvider _fileProvider;

	#region - Constructors - 

	public AssemblyLoadContextAssemblyLoader(IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

	#endregion - Constructors - 

	#region - Public Methods - 

	/// <summary>
	/// Loads the given assembly into a stream.
	/// </summary>
	/// <param name="assemblyFileName">File name of the assembly with path relative to FileProvider.</param>
	/// <returns></returns>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="Exception"></exception>
	public System.Reflection.Assembly Load(string assemblyFileName)
    {
        var file_info = _fileProvider.GetFileInfo(assemblyFileName);

        if (file_info.Exists == false)
        {
            throw new FileNotFoundException(assemblyFileName);
        }

        using var read_stream = file_info.CreateReadStream();

        try
        {
            return AssemblyLoadContext.Default.LoadFromStream(read_stream);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to load assembly {assemblyFileName}.", ex);
        }
    }

	#endregion - Public Methods - 
}
