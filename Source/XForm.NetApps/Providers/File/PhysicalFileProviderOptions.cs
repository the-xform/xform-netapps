// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

namespace XForm.NetApps.Providers.File;

/// <summary>
/// Options for loading a physical file.
/// </summary>
public class PhysicalFileProviderOptions : FileProviderOptions
{
    public string Root { get; set; } = string.Empty;
}

/// <summary>
/// Options for loading a file, resolved from configuration.
/// </summary>
public class FileProviderOptions
{
	/// <summary>
	/// The unique key for this file provider instance.
	/// </summary>
	public string Key { get; set; } = string.Empty;

	/// <summary>
	/// Is this the default file provider to be used when no specific file provider is specified?
	/// </summary>
	public bool IsDefault { get; set; }

	/// <summary>
	/// Is the file writable?
	/// </summary>
	public bool IsWritable { get; set; }

	/// <summary>
	/// The factory that will be used to create the file provider instance.
	/// </summary>
	public string Factory { get; set; } = string.Empty;
}
