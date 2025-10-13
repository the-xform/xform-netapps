// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using XForm.NetApps.Providers;

namespace XForm.NetApps.Interfaces;

/// <summary>
/// Generates a new sequential GUID each time NewSequentialGuid is called.
/// </summary>
public interface ISequentialGuidProvider
{
	/// <summary>
	/// Generates a new sequential GUID each time NewSequentialGuid is called.
	/// </summary>
	/// <param name="guidType"></param>
	/// <returns>Guid</returns>
	Guid NewGuid(SequentialGuidType guidType = SequentialGuidType.SequentialAtEndFromGuid);
}

