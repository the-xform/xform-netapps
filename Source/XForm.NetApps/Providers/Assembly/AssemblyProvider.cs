// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Reflection;
using XForm.Core.Interfaces;

namespace XForm.NetApps.Providers.Assembly;

/// <summary>
/// Assembly provider for the host.
/// </summary>
public class AssemblyProvider : IAssemblyProvider
{
    private readonly Dictionary<AssemblyName, System.Reflection.Assembly> _cachedAssemblies = new Dictionary<AssemblyName, System.Reflection.Assembly>();
    private readonly ILoadedAssembliesProvider _loadedAssembliesProvider;
    private readonly IAssemblyLoader _assemblyLoader;

    #region - Constructors - 

    public AssemblyProvider(
      ILoadedAssembliesProvider loadedAssembliesProvider,
      IAssemblyLoader assemblyLoader)
    {
        _loadedAssembliesProvider = loadedAssembliesProvider;
        _assemblyLoader = assemblyLoader;
    }

    public AssemblyProvider(IAssemblyLoader assemblyLoader)
        : this(new AppDomainLoadedAssembliesProvider(), assemblyLoader)
    {
    }

	#endregion - Constructors - 

	#region - Public Methods -

	/// <summary>
	/// Loads the given assembly.
	/// </summary>
	/// <param name="assemblyName"></param>
	/// <returns></returns>
	public System.Reflection.Assembly? Get(AssemblyName assemblyName)
    {
        if (assemblyName.Name == null)
        {
            return null;
        }

        if (_cachedAssemblies.ContainsKey(assemblyName))
        {
            return _cachedAssemblies[assemblyName];
        }

        var assemblies = _loadedAssembliesProvider.GetAll();
        var loaded_assembly = assemblies.FirstOrDefault(a => string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));

        if (loaded_assembly != null)
        {
            return DoUseLoadedAssembly(loaded_assembly, assemblyName);
        }

        return DoLoadAssembly(assemblyName);
    }

	#endregion - Public Methods -

	#region - Private Methods - 

	private System.Reflection.Assembly DoUseLoadedAssembly(System.Reflection.Assembly loadedAssembly, AssemblyName requestedAssembly)
    {
        var version = loadedAssembly.GetName().Version;

        if (version != null)
        {
            // Do not support if the requested assembly doesn't specify a version.
            if (requestedAssembly.Version == null)
            {
                throw new Exception($"Version must be specified when other versions of the dll are already loaded. Assembly: {loadedAssembly.GetName()}, Version: {version}");
            }

            // Lower version of an already loaded assembly is not supported.
            if (version.CompareTo(requestedAssembly.Version) > 0)
            {
                throw new Exception($"Loaded assemblies version is lower than assembly being requested. Assembly: {loadedAssembly.GetName()}, Version: {version}");
            }
        }

        _cachedAssemblies[requestedAssembly] = loadedAssembly;
        return loadedAssembly;
    }

    private System.Reflection.Assembly DoLoadAssembly(AssemblyName assemblyName)
    {
        try
        {
            var assembly = _assemblyLoader.Load($"{assemblyName.Name}.dll");
            _cachedAssemblies[assemblyName] = assembly;
            return assembly;
        }
        catch (FileNotFoundException ex)
        {
            throw new Exception("Unable to find requested assembly.", ex);
        }
        catch (FileLoadException ex)
        {
            throw new Exception("Unable to load requested assembly.", ex);
        }
    }

    #endregion - Private Methods - 
}
