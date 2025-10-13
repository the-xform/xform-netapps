using System.Reflection;

namespace XForm.NetApps.Tests.Unit;

public class AssemblyNameComparerTests
{
	private readonly AssemblyNameComparer _comparer = new AssemblyNameComparer();

	[Fact]
	public void Equals_SameReference_ReturnsTrue()
	{
		// Arrange
		var asm = new AssemblyName("TestAssembly");

		// Act
		var result = _comparer.Equals(asm, asm);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Equals_FirstNull_ReturnsFalse()
	{
		// Arrange
		AssemblyName? asm1 = null;
		var asm2 = new AssemblyName("TestAssembly");

		// Act
		var result = _comparer.Equals(asm1, asm2);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Equals_SecondNull_ReturnsFalse()
	{
		// Arrange
		var asm1 = new AssemblyName("TestAssembly");
		AssemblyName? asm2 = null;

		// Act
		var result = _comparer.Equals(asm1, asm2);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void Equals_SameFullNameDifferentCasing_ReturnsTrue()
	{
		// Arrange
		var asm1 = new AssemblyName("Test.Assembly");
		var asm2 = new AssemblyName("test.assembly");

		// Act
		var result = _comparer.Equals(asm1, asm2);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Equals_DifferentFullName_ReturnsFalse()
	{
		// Arrange
		var asm1 = new AssemblyName("Assembly.One");
		var asm2 = new AssemblyName("Assembly.Two");

		// Act
		var result = _comparer.Equals(asm1, asm2);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void GetHashCode_EqualAssemblies_HaveSameHashCode()
	{
		// Arrange
		var asm1 = new AssemblyName("Test.Assembly");
		var asm2 = new AssemblyName("Test.Assembly");

		// Act
		var hash1 = _comparer.GetHashCode(asm1);
		var hash2 = _comparer.GetHashCode(asm2);

		// Assert
		Assert.Equal(hash1, hash2);
	}

	[Fact]
	public void GetHashCode_DifferentAssemblies_HaveDifferentHashCodes()
	{
		// Arrange
		var asm1 = new AssemblyName("Assembly.One");
		var asm2 = new AssemblyName("Assembly.Two");

		// Act
		var hash1 = _comparer.GetHashCode(asm1);
		var hash2 = _comparer.GetHashCode(asm2);

		// Assert
		Assert.NotEqual(hash1, hash2);
	}
}
