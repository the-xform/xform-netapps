// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

namespace XForm.NetApps;

/// <summary>
/// State of the process for background workers.
/// </summary>
public enum ProcessState
{
    None = 0,
    NoRetry = 1,
    Retry = 2
}