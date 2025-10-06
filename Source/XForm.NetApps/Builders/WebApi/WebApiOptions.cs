// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

namespace XForm.NetApps.Builders.WinForms;

public class WebApiOptions : ServiceOptions
{
	public string ApiName { get; init; } = string.Empty;
	public string[] Args { get; init; } = Array.Empty<string>();
}
