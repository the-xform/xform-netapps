// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

namespace XForm.NetApps.Builders.WinService.Interfaces;

public interface IWorker
{
    /// <summary>
    /// Name of the worker.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Execute the background task with a cancellation token.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}
