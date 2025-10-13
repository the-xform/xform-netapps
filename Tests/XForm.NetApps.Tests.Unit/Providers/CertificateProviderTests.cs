using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Moq;
using XForm.NetApps.ConfigurationSettings;
using XForm.NetApps.Providers;

namespace XForm.NetApps.Tests.Unit.Providers;

public class CertificateProviderTests
{
	#region - Constructor Tests -

	[Fact]
	public void Constructor_WithNullCertificateSettings_ThrowsConfigurationErrorsException()
	{
		// Arrange
		// Get<T>() is an extension method from Microsoft.Extensions.Configuration.ConfigurationBinder. Moq cannot intercept extension methods directly because they are static methods, not virtual/overridable instance methods. That’s why it throws: System.NotSupportedException: Unsupported expression
		//var sectionMock = new Mock<IConfigurationSection>();
		//sectionMock.Setup(s => s.Get<CertificateSettings>()).Returns((CertificateSettings)null!);

		//var configMock = new Mock<IConfiguration>();
		//configMock.Setup(c => c.GetSection("CertificateSettings")).Returns(sectionMock.Object);

		// Arrange: create configuration without the section
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				// "CertificateSettings" is missing → should cause exception
			})
			.Build();

		// Act & Assert
		Assert.Throws<ConfigurationErrorsException>(() => new CertificateProvider(configuration));
	}

	[Fact]
	public void Constructor_DisabledProvider_DoesNotThrow()
	{
		// Arrange
		var settings = new CertificateSettings { IsEnabled = false, Thumbprints = [], CertStoreLocation = StoreLocation.LocalMachine.ToString(), CertStoreName = StoreName.My.ToString() };

		// Act
		var provider = new CertificateProvider(settings);

		// Assert
		Assert.NotNull(provider); // should initialize without exceptions
		Assert.NotNull(provider); // should initialize without exceptions

		Assert.Equal(StoreName.My, provider.CertStoreName); // should initialize without exceptions
		Assert.Equal(StoreLocation.LocalMachine, provider.CertStoreLocation); // should initialize without exceptions
	}

	[Fact]
	public void Constructor_EnabledProvider_DoesNotThrow_When()
	{
		// Arrange
		var settings = new CertificateSettings { IsEnabled = false, Thumbprints = [], CertStoreLocation = StoreLocation.CurrentUser.ToString(), CertStoreName = StoreName.Root.ToString() };

		// Act
		var provider = new CertificateProvider(settings);

		// Assert
		Assert.NotNull(provider); // should initialize without exceptions
		Assert.NotNull(provider); // should initialize without exceptions

		Assert.Equal(StoreName.Root, provider.CertStoreName); // should initialize without exceptions
		Assert.Equal(StoreLocation.CurrentUser, provider.CertStoreLocation); // should initialize without exceptions
	}

	#endregion - Constructor Tests -

	#region - GetCertificate Tests -

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

	#endregion - GetCertificate Tests -

	#region - ValidateCertificate Tests -

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

	#endregion - ValidateCertificate Tests -

	#region - ValidateCertificateChain Tests -

	[Fact]
	public void ValidateCertificateChain_NullCertificate_ThrowsArgumentNullException()
	{
		// Arrange
		var provider = new CertificateProvider(Array.Empty<string>());

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => provider.ValidateCertificateChain(null!, out _));
	}

	[Fact]
	public void ValidateCertificateChain_NullCertificate_ValidatesCert()
	{
		// Arrange
		(var rootCert, var caCert, var clientCert) = DoCreateCertificateChain();
		var chain_policy = new X509ChainPolicy
		{
			RevocationMode = X509RevocationMode.NoCheck,
			VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority, // To allow self-signed-root that is not installed in trusted store. This is for testing only.
			VerificationTimeIgnored = false
		};

		var provider = new CertificateProvider(Array.Empty<string>());

		// Act
		provider.AddCertificates([rootCert, caCert]);
		var is_valid = provider.ValidateCertificateChain(clientCert, out var chainStatus, chain_policy);

		// Assert
		Assert.True(is_valid);
		Assert.Equal(StoreName.My, provider.CertStoreName); // should initialize without exceptions
		Assert.Equal(StoreLocation.LocalMachine, provider.CertStoreLocation); // should initialize without exceptions
	}

	#endregion - ValidateCertificateChain Tests -

	#region - TryParse Tests -

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

	#endregion - TryParse Tests -

	#region - AddCertificates Tests -

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

	#endregion - AddCertificates Tests -

	#region - Private Methods -

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
		sectionMock.Setup(s => s.Get<CertificateSettings>()).Returns(new CertificateSettings
		{
			IsEnabled = true,
			CertStoreLocation = "CurrentUser",
			CertStoreName = "My",
			Thumbprints = Array.Empty<string>()
		});

		var configMock = new Mock<IConfiguration>();
		configMock.Setup(c => c.GetSection("CertificateSettings")).Returns(sectionMock.Object);

		return configMock.Object;
	}

	public static (X509Certificate2 root, X509Certificate2 ca, X509Certificate2 client)
		DoCreateCertificateChain(string rootName = "RootCA", string caName = "IntermediateCA", string clientName = "ClientCert")
	{
		// 1ROOT CA
		using var rootKey = RSA.Create(4096);
		var rootReq = new CertificateRequest(
			new X500DistinguishedName($"CN={rootName}, O=ExampleOrg, C=US"),
			rootKey,
			HashAlgorithmName.SHA256,
			RSASignaturePadding.Pkcs1);

		rootReq.CertificateExtensions.Add(
			new X509BasicConstraintsExtension(true, false, 0, true)); // CA=true
		rootReq.CertificateExtensions.Add(
			new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
		rootReq.CertificateExtensions.Add(
			new X509SubjectKeyIdentifierExtension(rootReq.PublicKey, false));

		var rootCert = rootReq.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(20));

		// Export to persist private key - not needed because CreateSelfSigned already contains the private key.
		//var rootCertWithKey = rootCert.CopyWithPrivateKey(rootKey);

		// INTERMEDIATE / CA
		using var caKey = RSA.Create(4096);
		var caReq = new CertificateRequest(
			new X500DistinguishedName($"CN={caName}, O=ExampleOrg, C=US"),
			caKey,
			HashAlgorithmName.SHA256,
			RSASignaturePadding.Pkcs1);

		caReq.CertificateExtensions.Add(
			new X509BasicConstraintsExtension(true, false, 0, true)); // CA=true
		caReq.CertificateExtensions.Add(
			new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, true));
		caReq.CertificateExtensions.Add(
			new X509SubjectKeyIdentifierExtension(caReq.PublicKey, false));

		var caCert = caReq.Create(rootCert, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(10), DoGenerateSerialNumber());

		var caCertWithKey = caCert.CopyWithPrivateKey(caKey);

		// CLIENT CERTIFICATE
		using var clientKey = RSA.Create(2048);
		var clientReq = new CertificateRequest(
			new X500DistinguishedName($"CN={clientName}, O=ExampleOrg, C=US"),
			clientKey,
			HashAlgorithmName.SHA256,
			RSASignaturePadding.Pkcs1);

		clientReq.CertificateExtensions.Add(
			new X509BasicConstraintsExtension(false, false, 0, false)); // Not a CA
		clientReq.CertificateExtensions.Add(
			new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true));
		clientReq.CertificateExtensions.Add(
			new X509EnhancedKeyUsageExtension(
				new OidCollection {
					new Oid("1.3.6.1.5.5.7.3.2") // Client Authentication
				}, true));
		clientReq.CertificateExtensions.Add(
			new X509SubjectKeyIdentifierExtension(clientReq.PublicKey, false));

		var clientCert = clientReq.Create(caCertWithKey, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5), DoGenerateSerialNumber());
		var clientCertWithKey = clientCert.CopyWithPrivateKey(clientKey);

		// Return all
		return (rootCert, caCertWithKey, clientCert);
	}

	private static byte[] DoGenerateSerialNumber()
	{
		var serial = new byte[16];
		RandomNumberGenerator.Fill(serial);
		return serial;
	}
	#endregion - Private Methods -
}
