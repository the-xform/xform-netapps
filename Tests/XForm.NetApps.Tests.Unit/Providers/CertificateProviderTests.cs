using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Moq;
using XForm.NetApps.ConfigurationSettings;
using XForm.NetApps.Providers;

namespace XForm.NetApps.Tests.Unit.Providers
{
	public class CertificateProviderTests
	{
		#region Helper Methods

		private X509Certificate2 CreateSelfSignedCertificate(string subjectName, DateTimeOffset notBefore, DateTimeOffset notAfter)
		{
			using var rsa = RSA.Create(2048);
			var req = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
			return req.CreateSelfSigned(notBefore, notAfter);
		}

		private IConfiguration CreateValidConfiguration()
		{
			var sectionMock = new Mock<IConfigurationSection>();
			sectionMock.Setup(s => s.Exists()).Returns(true);
			sectionMock.Setup(s => s.Get<CertificatesSettings>()).Returns(new CertificatesSettings
			{
				IsEnabled = true,
				StoreLocation = "CurrentUser",
				StoreName = "My",
				Thumbprints = Array.Empty<string>()
			});

			var configMock = new Mock<IConfiguration>();
			configMock.Setup(c => c.GetSection("CertificatesSettings")).Returns(sectionMock.Object);

			return configMock.Object;
		}

		#endregion

		#region Constructor Tests

		[Fact]
		public void Constructor_WithNullCertificatesSettings_ThrowsConfigurationErrorsException()
		{
			// Arrange
			// Get<T>() is an extension method from Microsoft.Extensions.Configuration.ConfigurationBinder. Moq cannot intercept extension methods directly because they are static methods, not virtual/overridable instance methods. That’s why it throws: System.NotSupportedException: Unsupported expression
			//var sectionMock = new Mock<IConfigurationSection>();
			//sectionMock.Setup(s => s.Get<CertificatesSettings>()).Returns((CertificatesSettings)null!);

			//var configMock = new Mock<IConfiguration>();
			//configMock.Setup(c => c.GetSection("CertificatesSettings")).Returns(sectionMock.Object);


			// Arrange: create configuration without the section
			var configuration = new ConfigurationBuilder()
				.AddInMemoryCollection(new Dictionary<string, string?>
				{
					// "CertificatesSettings" is missing → should cause exception
				})
				.Build();

			// Act & Assert
			Assert.Throws<ConfigurationErrorsException>(() => new CertificateProvider(configuration));
		}

		[Fact]
		public void Constructor_DisabledProvider_DoesNotThrow()
		{
			// Arrange
			var settings = new CertificatesSettings { IsEnabled = false, Thumbprints = [] };

			// Act
			var provider = new CertificateProvider(settings);

			// Assert
			Assert.NotNull(provider); // should initialize without exceptions
		}

		#endregion

		#region GetCertificate Tests

		[Fact]
		public void GetCertificate_WhenNotFound_ReturnsNull()
		{
			// Arrange
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act
			var cert = provider.GetCertificate("NONEXISTENT");

			// Assert
			Assert.Null(cert);
		}

		[Fact]
		public void GetCertificate_WhenFound_ReturnsCertificate()
		{
			// Arrange
			var cert = CreateSelfSignedCertificate("Test", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
			var provider = new CertificateProvider(new List<X509Certificate2> { cert });

			// Act
			var found = provider.GetCertificate(cert.Thumbprint!);

			// Assert
			Assert.NotNull(found);
			Assert.Equal(cert.Thumbprint, found!.Thumbprint);
		}

		#endregion

		#region ValidateCertificate Tests

		[Fact]
		public void ValidateCertificate_NullCertificate_ReturnsFalse()
		{
			// Arrange
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act
			var result = provider.ValidateCertificate(null);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void ValidateCertificate_ExpiredCertificate_ReturnsFalse()
		{
			// Arrange
			var expiredCert = CreateSelfSignedCertificate("Expired", DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow.AddDays(-1));
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act
			var result = provider.ValidateCertificate(expiredCert);

			// Assert
			Assert.False(result);
		}

		[Fact]
		public void ValidateCertificate_ValidCertificate_ReturnsTrue()
		{
			// Arrange
			var validCert = CreateSelfSignedCertificate("Valid", DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act
			var result = provider.ValidateCertificate(validCert);

			// Assert
			Assert.True(result);
		}

		#endregion

		#region ValidateCertificateChain Tests

		[Fact]
		public void ValidateCertificateChain_NullCertificate_ThrowsArgumentNullException()
		{
			// Arrange
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() => provider.ValidateCertificateChain(null!, out _));
		}

		#endregion

		#region TryParse Tests

		[Theory]
		[InlineData("CurrentUser", StoreLocation.CurrentUser)]
		[InlineData("LocalMachine", StoreLocation.LocalMachine)]
		[InlineData("user", StoreLocation.CurrentUser)]
		[InlineData("machine", StoreLocation.LocalMachine)]
		public void TryParseStoreLocation_ValidInputs_ReturnsTrue(string input, StoreLocation expected)
		{
			// Arrange
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act
			var success = provider.TryParseStoreLocation(input, out var storeLocation);

			// Assert
			Assert.True(success);
			Assert.Equal(expected, storeLocation);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("Unknown")]
		public void TryParseStoreLocation_InvalidInputs_ReturnsFalse(string input)
		{
			// Arrange
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act
			var success = provider.TryParseStoreLocation(input, out var storeLocation);

			// Assert
			Assert.False(success);
		}

		[Theory]
		[InlineData("My", StoreName.My)]
		[InlineData("Root", StoreName.Root)]
		[InlineData("TrustedPeople", StoreName.TrustedPeople)]
		[InlineData("TrustedPublisher", StoreName.TrustedPublisher)]
		[InlineData("CA", StoreName.CertificateAuthority)]
		public void TryParseStoreName_ValidInputs_ReturnsTrue(string input, StoreName expected)
		{
			// Arrange
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act
			var success = provider.TryParseStoreName(input, out var storeName);

			// Assert
			Assert.True(success);
			Assert.Equal(expected, storeName);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("Unknown")]
		public void TryParseStoreName_InvalidInputs_ReturnsFalse(string input)
		{
			// Arrange
			var provider = new CertificateProvider(Array.Empty<string>());

			// Act
			var success = provider.TryParseStoreName(input, out var storeName);

			// Assert
			Assert.False(success);
		}

		#endregion

		#region AddCertificates Tests

		[Fact]
		public void AddCertificates_AllThumbprintsNotFound_ReturnsAllInNotFound()
		{
			// Arrange
			var provider = new CertificateProvider(Array.Empty<string>(), isEnabled: true);
			var thumbprints = new List<string> { "TP1", "TP2" };

			// Act
			provider.AddCertificates(thumbprints, out var notFound);

			// Assert
			Assert.Equal(2, notFound.Count);
			Assert.Contains("TP1", notFound);
			Assert.Contains("TP2", notFound);
		}

		#endregion
	}
}
