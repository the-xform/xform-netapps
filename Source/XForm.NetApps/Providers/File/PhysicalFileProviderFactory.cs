// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using XForm.Core.Interfaces;
using XForm.Utilities;

namespace XForm.NetApps.Providers.File;

/// <summary>
/// A factory that creates a file provider.
/// </summary>
public class PhysicalFileProviderFactory : IFileProviderFactory
{
	private readonly string _root;
	private const string BaseDirectoryConstant = "BASE_DIRECTORY"; // Put this key in the config if you want to use the BaseDirectory from AppDomain.

	#region - Constructors -

	public PhysicalFileProviderFactory(IConfiguration configuration)
	{
		var physical_file_options = configuration.Get<PhysicalFileProviderOptions>() ?? throw new ConfigurationErrorsException($"Unable to create {nameof(PhysicalFileProviderOptions)} from configuration.");

		var root = physical_file_options.Root;

		if (string.IsNullOrEmpty(root))
		{
			if (string.Equals(root, BaseDirectoryConstant, StringComparison.OrdinalIgnoreCase))
			{
				_root = AppDomain.CurrentDomain.BaseDirectory;
			}
			else if (Path.IsPathRooted(root))
			{
				_root = root;
			}
			else
			{
				_root = Path.Combine(LocationUtilities.GetEntryAssemblyDirectory(), root);
			}
		}
		else
		{
			_root = AppDomain.CurrentDomain.BaseDirectory;
		}
	}

	#endregion - Constructors -

	#region - Public Methods  -

	/// <summary>
	/// Creates and returns a new instance of a writable file provider rooted at the specified directory.
	/// </summary>
	/// <remarks>The returned file provider allows read and write operations within the scope of the specified
	/// root directory.</remarks>
	/// <returns>An <see cref="IFileProvider"/> implementation that provides access to the file system at the configured root
	/// directory.</returns>
	public IFileProvider Create()
	{
		return new PhysicalFileWritableProvider(_root);
	}

	/// <summary>
	/// Creates and returns an instance of a writable file provider.
	/// </summary>
	/// <remarks>The returned file provider allows writing operations within the root directory specified
	/// during the initialization of the current instance.</remarks>
	/// <returns>An <see cref="IWritableFileProvider"/> that provides writable access to files.</returns>
	public IWritableFileProvider CreateWritable()
	{
		return new PhysicalFileWritableProvider(_root);
	}

	#endregion - Public Methods  -
}
