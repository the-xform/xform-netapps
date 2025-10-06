// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using XForm.Core.Interfaces;

namespace XForm.NetApps.Builders.WinService;

/// <summary>
/// Instance creator class for loading and instantiating types at runtime.
/// </summary>
public class InstanceCreator : IInstanceCreator
{
	#region - Public Methods -

	/// <summary>
	/// Creates an instance of the specified type using the provided constructor parameters.
	/// </summary>
	/// <typeparam name="TType"></typeparam>
	/// <param name="type"></param>
	/// <param name="constructorParameters"></param>
	/// <returns></returns>
	public TType InstantiateType<TType>(string type, params object[] constructorParameters) where TType : class
    {
        return InstanceCreatorHelper.InstantiateType<TType>(type, constructorParameters);
    }

	#endregion - Public Methods -
}

/// <summary>
/// Instance creator class for loading and instantiating the types that require parameters to be resoved from service collection at runtime.
/// </summary>
public class ResolvedInstanceCreator : IInstanceCreator
{
    private readonly IServiceProvider _serviceProvider;

	#region - Contructors -

	public ResolvedInstanceCreator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

	#endregion - Contructors -

	#region - Public Methods -

	/// <summary>
	/// Creates an instance of the specified type, resolving constructor parameters from the service provider.
	/// </summary>
	/// <typeparam name="TType"></typeparam>
	/// <param name="type"></param>
	/// <param name="constructorParameters"></param>
	/// <returns></returns>
	public TType InstantiateType<TType>(string type, params object[] constructorParameters) where TType : class
    {
        return InstanceCreatorHelper.InstantiateType<TType>(_serviceProvider, type, constructorParameters);
    }

	#endregion - Public Methods -
}

public static class InstanceCreatorHelper
{
	#region - Public Methods -

	/// <summary>
	/// Creates an instance of the specified type using the provided constructor parameters.
	/// </summary>
	/// <typeparam name="TType"></typeparam>
	/// <param name="typeString"></param>
	/// <param name="constructorParameters"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static TType InstantiateType<TType>(string typeString, params object[] constructorParameters) where TType : class
    {
        var type = GetType(typeString);

        TType? instance = null;
        Exception? exception = null;

        try
        {
            instance = Activator.CreateInstance(type, constructorParameters) as TType;
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (instance == null)
        {
            throw new Exception($"Unable to instantiate {typeString} as {typeof(TType).AssemblyQualifiedName}", exception);
        }

        return instance;
    }

	/// <summary>
	/// creates an instance of the specified type, resolving constructor parameters from the service provider.
	/// </summary>
	/// <typeparam name="TType"></typeparam>
	/// <param name="serviceProvider"></param>
	/// <param name="typeString"></param>
	/// <param name="constructorParameters"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static TType InstantiateType<TType>(IServiceProvider serviceProvider, string typeString, params object[] constructorParameters) where TType : class
    {
        var type = GetType(typeString);

        TType? instance = null;
        Exception? exception = null;

        try
        {
            instance = ActivatorUtilities.CreateInstance(serviceProvider, type, constructorParameters) as TType;
        }
        catch
        {
            throw new Exception($"Unable to instantiate {typeString} as {typeof(TType).AssemblyQualifiedName}", exception);
        }

        if (instance == null)
        {
            throw new Exception($"Unable to instantiate {typeString} as {typeof(TType).AssemblyQualifiedName}", exception);
        }

        return instance;
    }

	#endregion - Public Methods -

    #region - Private Methods -

	private static Type GetType(string typeString)
    {
        Type? type = null;
        Exception? exception = null;

        try
        {
            type = Type.GetType(typeString, AssemblyLoadContext.Default.LoadFromAssemblyName, null);
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (type == null)
        {
            throw new Exception($"Unable to get type {typeString}.", exception);
        }

        return type;
    }

	#endregion - Private Methods -
}