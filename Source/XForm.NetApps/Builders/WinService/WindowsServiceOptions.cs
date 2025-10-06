// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using XForm.NetApps.Builders;

namespace XForm.NetApps.Hosts.WinService;

public class WindowsServiceOptions : ServiceOptions
{
    public string ServiceName { get; init; } = string.Empty;
	public string[] Args { get; init; } = Array.Empty<string>();
}
