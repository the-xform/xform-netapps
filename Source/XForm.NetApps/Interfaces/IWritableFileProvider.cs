// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.FileProviders;

namespace XForm.Core.Interfaces;

/// <summary>
/// Custom file provider based on Microsoft's IFileProvider.
/// </summary>
public interface IWritableFileProvider : IFileProvider
{
    /// <summary>
    /// Creates a directory.
    /// </summary>
    /// <param name="subPath"></param>
    void CreateDirectory(string subPath);

    /// <summary>
    /// Create a write-stream.
    /// </summary>
    /// <param name="subPath"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    Stream CreateWriteStream(string subPath, FileMode mode);
}
