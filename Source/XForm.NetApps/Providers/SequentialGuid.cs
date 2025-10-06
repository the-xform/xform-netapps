// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Security.Cryptography;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Providers;

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


public class SequentialGuidProvider : ISequentialGuidProvider
{
	#region - Public Methods -

	/// <summary>
	/// Generates a new sequential GUID each time NewSequentialGuid is called.
	/// </summary>
	/// <param name="guidType"></param>
	/// <returns>Guid</returns>
	public Guid NewGuid(SequentialGuidType guidType = SequentialGuidType.SequentialAtEndFromGuid)
	{
		return SequentialGuidGeneratorHelper.NewGuid(guidType);
	}

	#endregion - Public Methods -
}

public enum SequentialGuidType
{
	SequentialAsString,
	SequentialAsBinary,
	SequentialAtEnd,

	/// <summary>Good for MS SQL server.</summary>
	/// Added to skip RNG and just manipulate a System.Guid generated one instead.
	/// It is about 17x faster and preserves the 6 bits of "variant" info in GUID.
	SequentialAtEndFromGuid,
}

/// <summary>
/// Adapted from article "GUIDs as fast primary keys under multiple databases" by Jeromy Todd on CodeProject.
/// http://www.codeproject.com/Articles/388157/GUIDs-as-fast-primary-keys-under-multiple-database
/// </summary>
public static class SequentialGuidGeneratorHelper
{
	private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

	#region - Public Methods -

	/// <summary>
	/// Generates a new sequential GUID each time NewSequentialGuid is called.
	/// </summary>
	/// <param name="guidType"></param>
	/// <returns></returns>
	public static Guid NewGuid(SequentialGuidType guidType = SequentialGuidType.SequentialAtEndFromGuid)
	{
		byte[]? guid_bytes = null;

		long ticks = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
		byte[] ticks_bytes = BitConverter.GetBytes(ticks);

		if (BitConverter.IsLittleEndian)
		{
			Array.Reverse(ticks_bytes);
		}

		switch (guidType)
		{
			case SequentialGuidType.SequentialAsString:
			case SequentialGuidType.SequentialAsBinary:
			{
				byte[] random_bytes = new byte[10];
				_rng.GetBytes(random_bytes);
				guid_bytes = new byte[16];
				Buffer.BlockCopy(ticks_bytes, 2, guid_bytes, 0, 6);
				Buffer.BlockCopy(random_bytes, 0, guid_bytes, 6, 10);

				// If formatting as a string, we have to reverse the order
				// of the Data1 and Data2 blocks on little-endian systems.
				if (guidType == SequentialGuidType.SequentialAsString && BitConverter.IsLittleEndian)
				{
					Array.Reverse(guid_bytes, 0, 4);
					Array.Reverse(guid_bytes, 4, 2);
				}
				break;
			}
			case SequentialGuidType.SequentialAtEnd:
			{
				byte[] random_bytes = new byte[10];
				_rng.GetBytes(random_bytes);
				guid_bytes = new byte[16];
				Buffer.BlockCopy(random_bytes, 0, guid_bytes, 0, 10);
				Buffer.BlockCopy(ticks_bytes, 2, guid_bytes, 10, 6);
				break;
			}
			case SequentialGuidType.SequentialAtEndFromGuid:
			{
				Guid guid = Guid.NewGuid();
				guid_bytes = guid.ToByteArray();
				Buffer.BlockCopy(ticks_bytes, 2, guid_bytes, 10, 6);
				break;
			}
		}

		// Ensure btes is not null for the next step.
		Xssert.IsNotNull(guid_bytes);

		return new Guid(guid_bytes);
	}

	#endregion - Public Methods -
}

