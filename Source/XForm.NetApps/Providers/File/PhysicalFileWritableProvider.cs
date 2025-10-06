// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.FileProviders;
using XForm.Core.Interfaces;

namespace XForm.NetApps.Providers.File;

/// <summary>
/// Physical file provider.
/// </summary>
public class PhysicalFileWritableProvider : PhysicalFileProvider, IWritableFileProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PhysicalFileWritableProvider"/> class with the specified root
    /// directory.
    /// </summary>
    /// <remarks>The <see cref="PhysicalFileWritableProvider"/> class provides functionality for managing
    /// writable file operations within the specified root directory. Ensure that the application has appropriate
    /// permissions to access and modify files in the specified directory.</remarks>
    /// <param name="root">The root directory that this provider will use as the base path for file operations. Must be a valid, non-null,
    /// and non-empty path.</param>
    public PhysicalFileWritableProvider(string root)
        : base(root) { }

    /// <summary>
    /// Creates a directory.
    /// </summary>
    /// <param name="subPath"></param>
    public void CreateDirectory(string subPath)
    {
        _ = Directory.CreateDirectory(Path.Combine(Root, subPath));
    }

    /// <summary>
    /// Creates a writing stream for the given file.
    /// </summary>
    /// <param name="subPath"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public Stream CreateWriteStream(string subPath, FileMode mode)
    {
        var path = Path.Combine(Root, subPath);
        return new FileStream(path, mode);
    }
}
