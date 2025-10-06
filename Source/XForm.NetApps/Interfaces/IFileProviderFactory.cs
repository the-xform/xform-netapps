// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using Microsoft.Extensions.FileProviders;

namespace XForm.Core.Interfaces;

/// <summary>
/// File provider factory interface.
/// </summary>
public interface IFileProviderFactory
{
    /// <summary>
    /// Creates a file provider.
    /// </summary>
    /// <returns></returns>
    IFileProvider Create();
}
