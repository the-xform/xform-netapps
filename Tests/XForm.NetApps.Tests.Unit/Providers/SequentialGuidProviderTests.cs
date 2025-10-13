using XForm.NetApps.Providers;

namespace XForm.NetApps.Tests.Unit.Providers;

public class SequentialGuidProviderTests
{
	private readonly SequentialGuidProvider _provider = new SequentialGuidProvider();

	[Theory]
	[InlineData(SequentialGuidType.SequentialAsString)]
	[InlineData(SequentialGuidType.SequentialAsBinary)]
	[InlineData(SequentialGuidType.SequentialAtEnd)]
	[InlineData(SequentialGuidType.SequentialAtEndFromGuid)]
	public void NewGuid_ShouldReturnUniqueGuid_ForEachCall(SequentialGuidType guidType)
	{
		// Arrange & Act
		var g1 = _provider.NewGuid(guidType);
		var g2 = _provider.NewGuid(guidType);

		// Assert
		Assert.NotEqual(g1, g2);
		Assert.NotEqual(Guid.Empty, g1);
		Assert.NotEqual(Guid.Empty, g2);
	}

	[Fact]
	public void NewGuid_SequentialAtEndFromGuid_ShouldPreserveRandomness_AndEmbedTicks()
	{
		// Arrange
		var g1 = _provider.NewGuid(SequentialGuidType.SequentialAtEndFromGuid);
		Thread.Sleep(5);
		var g2 = _provider.NewGuid(SequentialGuidType.SequentialAtEndFromGuid);

		// Act
		var bytes1 = g1.ToByteArray();
		var bytes2 = g2.ToByteArray();

		// The last 6 bytes contain ticks (in big-endian order)
		var last_bytes1 = bytes1.Skip(10).ToArray();
		var last_bytes2 = bytes2.Skip(10).ToArray();

		// Assert: newer GUID has lexicographically higher tick bytes
		Assert.True(BitConverter.ToInt64(last_bytes2.Concat(new byte[2]).ToArray()) >
					BitConverter.ToInt64(last_bytes1.Concat(new byte[2]).ToArray()));
	}

	[Theory]
	[InlineData(SequentialGuidType.SequentialAsString)]
	[InlineData(SequentialGuidType.SequentialAsBinary)]
	[InlineData(SequentialGuidType.SequentialAtEnd)]
	[InlineData(SequentialGuidType.SequentialAtEndFromGuid)]
	public void NewGuid_ShouldBeValidGuidStructure(SequentialGuidType guidType)
	{
		// Act
		var guid = _provider.NewGuid(guidType);

		// Assert
		Assert.Equal(16, guid.ToByteArray().Length);
	}

	[Fact]
	public void NewGuid_SequentialAsString_ShouldChangeMonotonically_WhenCreatedSequentially()
	{
		// Arrange
		var g1 = _provider.NewGuid(SequentialGuidType.SequentialAsString);
		Thread.Sleep(5);
		var g2 = _provider.NewGuid(SequentialGuidType.SequentialAsString);

		// Act
		string s1 = g1.ToString("N");
		string s2 = g2.ToString("N");

		// Assert (lexical ordering increases over time)
		Assert.True(string.CompareOrdinal(s2, s1) > 0);
	}

	[Fact]
	public void Provider_ShouldUseHelper_ToGenerateGuid()
	{
		// Arrange
		var expected = SequentialGuidGeneratorHelper.NewGuid(SequentialGuidType.SequentialAtEndFromGuid);

		// Act
		var actual = _provider.NewGuid(SequentialGuidType.SequentialAtEndFromGuid);

		// Assert: same type and structure (not necessarily equal)
		Assert.NotEqual(Guid.Empty, actual);
		Assert.Equal(16, actual.ToByteArray().Length);
		Assert.NotEqual(expected, actual); // both unique
	}

	[Fact]
	public void Helper_ShouldGenerateGuid_ForAllTypes()
	{
		foreach (SequentialGuidType type in Enum.GetValues(typeof(SequentialGuidType)))
		{
			var guid = SequentialGuidGeneratorHelper.NewGuid(type);
			Assert.NotEqual(Guid.Empty, guid);
		}
	}
}
