using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using XForm.NetApps.ConfigurationSettings;
using XForm.NetApps.Interfaces;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Providers;

public class CertificateProvider : ICertificateProvider
{
	private readonly CertificatesSettings? _certificateSettings;

	private readonly bool _isProviderEnabled;
	private readonly List<X509Certificate2> _allCertificates;
	private readonly StoreLocation _storeLocation;
	private readonly StoreName _storeName;
	private readonly string? _parentAki;

	public static X509ChainPolicy DefaultChainPolicy { get; private set; } = new X509ChainPolicy
	{
		RevocationMode = X509RevocationMode.NoCheck,
		VerificationFlags = X509VerificationFlags.NoFlag,
		VerificationTimeIgnored = false
	};

	#region - Constructors -

	/// <summary>
	/// Constructor that initializes a new instance of the <see cref="CertificateProvider"/> class with configuration settings. The configuration must contain a <see cref="CertificatesSettings"/> section.
	/// </summary>
	/// <param name="configuration"></param>
	/// <exception cref="ConfigurationErrorsException"></exception>
	public CertificateProvider(IConfiguration configuration)
	{
		_allCertificates = new List<X509Certificate2>();

		var certificates_config_section = configuration.GetSection("CertificatesSettings");
		if (certificates_config_section.Exists() == false)
		{
			throw new ConfigurationErrorsException($"No 'CertificatesSettings' section found in the configuration.");
		}

		var certificate_settings = certificates_config_section.Get<CertificatesSettings>();
		if (certificate_settings == null)
		{
			throw new ConfigurationErrorsException($"Unable to parse 'CertificatesSettings' section from the configuration.");
		}

		_certificateSettings = certificate_settings;
		_isProviderEnabled = _certificateSettings.IsEnabled;

		if (_isProviderEnabled == false)
		{
			return; // Provider is disabled, no need to load certificates.
		}

		_parentAki = _certificateSettings.CaAuthorityKeyIdentifier;

		if (_certificateSettings.StoreLocation.HasSomething())
		{
			if (TryParseStoreLocation(_certificateSettings.StoreLocation, out _storeLocation) == false)
			{
				throw new ConfigurationErrorsException($"Invalid certificate store location '{_certificateSettings.StoreLocation}'. Possible values are 'CurrentUser' or 'LocalMachine'.");
			}
		}
		else
		{
			_storeLocation = StoreLocation.LocalMachine; // Default value
		}

		if (_certificateSettings.StoreName.HasSomething())
		{
			if (TryParseStoreName(_certificateSettings.StoreName, out _storeName) == false)
			{
				throw new ConfigurationErrorsException($"Invalid certificate store name '{_certificateSettings.StoreName}'. Possible values are 'My', 'Root', 'CA', 'AuthRoot', 'TrustedPeople', or 'TrustedPublisher'.");
			}
		}
		else
		{
			_storeName = StoreName.My; // Default value
		}

		_allCertificates.AddRange(DoGetCertificates(_certificateSettings.Thumbprints));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CertificateProvider"/> class.
	/// </summary>
	/// <param name="certificatesSettings">The <see cref="CertificatesSettings"/> object</param>
	/// <param name="storeLocation">Possible values are 'CurrentUser' or 'LocalMachine'.</param>
	/// <param name="storeName">Possible values are 'My', 'Root', 'CA', 'AuthRoot', 'TrustedPeople', or 'TrustedPublisher'.</param>
	/// <exception cref="ConfigurationErrorsException"></exception>
	public CertificateProvider(CertificatesSettings certificatesSettings)
		: this(certificatesSettings.Thumbprints, certificatesSettings.StoreLocation, certificatesSettings.StoreName, certificatesSettings.CaAuthorityKeyIdentifier, certificatesSettings.IsEnabled)
	{
		_certificateSettings = certificatesSettings;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="CertificateProvider"/> class.
	/// </summary>
	/// <param name="thumbprints"></param>
	/// <param name="storeLocation"></param>
	/// <param name="storeName"></param>
	/// <param name="parentAki"></param>
	/// <exception cref="ConfigurationErrorsException"></exception>
	public CertificateProvider(string[] thumbprints, string? storeLocation = null, string? storeName = null, string? parentAki = null, bool isEnabled = true)
	{
		_allCertificates = new List<X509Certificate2>();
		_parentAki = parentAki;
		_isProviderEnabled = isEnabled;

		if (_isProviderEnabled == true)
		{
			if (storeLocation.HasSomething())
			{
				if (TryParseStoreLocation(storeLocation, out _storeLocation) == false)
				{
					throw new ConfigurationErrorsException($"Invalid certificate store location '{storeLocation}'. Possible values are 'CurrentUser' or 'LocalMachine'.");
				}
			}
			else
			{
				_storeLocation = StoreLocation.LocalMachine; // Default value
			}

			if (storeName.HasSomething())
			{
				if (TryParseStoreName(storeName, out _storeName) == false)
				{
					throw new ConfigurationErrorsException($"Invalid certificate store name '{storeName}'. Possible values are 'My', 'Root', 'CA', 'AuthRoot', 'TrustedPeople', or 'TrustedPublisher'.");
				}
			}
			else
			{
				_storeName = StoreName.My; // Default value
			}

			_allCertificates.AddRange(DoGetCertificates(thumbprints));
		}
	}


	/// <summary>
	/// Initializes a new instance of the <see cref="CertificateProvider"/> class.
	/// </summary>
	/// <param name="certificates"></param>
	public CertificateProvider(List<X509Certificate2> certificates)
	{
		_allCertificates = new List<X509Certificate2>(certificates);
		_isProviderEnabled = true;
	}

	#endregion - Constructors -

	/// <summary>
	/// Gets a certificate from the loaded collection of certificates.
	/// </summary>
	/// <param name="thumbprint"></param>
	/// <returns></returns>
	public X509Certificate2? GetCertificate(string thumbprint)
	{
		return _allCertificates.FirstOrDefault(cert => cert.Thumbprint?.Equals(thumbprint, StringComparison.InvariantCultureIgnoreCase) == true);
	}

	/// <summary>
	/// Gets a certificate from the specified store.
	/// </summary>
	/// <param name="thumbprint"></param>
	/// <param name="storeLocation"></param>
	/// <param name="storeName"></param>
	/// <returns></returns>
	public X509Certificate2? GetCertificate(string thumbprint, StoreLocation storeLocation, StoreName storeName)
	{
		_ = DoTryGetCertificate(thumbprint, storeLocation, storeName, out var cert);

		return cert;
	}

	/// <summary>
	/// Add additional certificates to the existing collection of certificates.
	/// </summary>
	/// <param name="thumbprints"></param>
	/// <param name="notFoundThumbprints"></param>
	public void AddCertificates(List<string> thumbprints, out List<string> notFoundThumbprints)
	{
		List<string> not_found_tp = new List<string>();

		thumbprints.ForEach(tp =>
		{
			if (DoTryGetCertificate(tp, _storeLocation, _storeName, out var cert) == true)
			{
				_allCertificates.Add(cert!);
			}
			else
			{
				not_found_tp.Add(tp);
			}
		});

		notFoundThumbprints = not_found_tp;
	}

	/// <summary>
	/// Add additional certificates to the existing collection of certificates.
	/// </summary>
	/// <param name="certificates"></param>
	public void AddCertificates(List<X509Certificate2> certificates)
	{
		certificates.ForEach(cert =>
		{
			_allCertificates.Add(cert);
		});
	}

	/// <summary>
	/// Validates the specified certificate against a certificate chain, with the given certificate policy. If none specified, uses the default policy.
	/// </summary>
	/// <param name="certificate"></param>
	/// <param name="chainPolicy"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	public bool ValidateCertificateChain(X509Certificate2 certificate, out X509ChainStatus[] chainStatus, X509ChainPolicy? chainPolicy = null)
	{
		if (certificate == null)
		{
			throw new ArgumentNullException(nameof(certificate));
		}

		chainStatus = Array.Empty<X509ChainStatus>();

		using var chain = new X509Chain
		{
			ChainPolicy = chainPolicy ?? new X509ChainPolicy // Cannot assing DefaultChainPolicy directly because we're adding to chainPolicy.ExtraStore in the next step and that will modify the DefaultChainPolicy which is not desired.
			{
				RevocationMode = DefaultChainPolicy.RevocationMode,
				VerificationFlags = DefaultChainPolicy.VerificationFlags,
				VerificationTimeIgnored = DefaultChainPolicy.VerificationTimeIgnored
			}
		};

		// Add parent certificates to the chain.
		_allCertificates.ForEach(parentCert => { chain.ChainPolicy.ExtraStore.Add(parentCert); });

		// Build and validate the certificate chain.
		var is_valid = chain.Build(certificate);

		chainStatus = chain.ChainStatus;

		return is_valid;
	}

	/// <summary>
	/// Validates the specified certificate for expiration date.
	/// </summary>
	/// <param name="certificate"></param>
	/// <returns></returns>
	public bool ValidateCertificate(X509Certificate2? certificate)
	{
		if (certificate == null)
		{
			return false;
		}

		var now = DateTime.UtcNow;
		if (now < certificate.NotBefore.ToUniversalTime() || now > certificate.NotAfter.ToUniversalTime())
		{
			return false;
		}

		return true;
	}

	/// <summary>
	/// Validates the Subject Key Identifier (SKI) of an X509Certificate2.
	/// </summary>
	/// <param name="certificate">The certificate to validate.</param>
	/// <param name="expectedSki">Optional expected SKI value to compare against (hex string).</param>
	/// <param name="issuerCertificate">Optional issuer certificate to compare SKI with its AKI.</param>
	/// <returns>True if SKI is valid (and matches expected/issuer if provided).</returns>
	public bool ValidateSubjectKeyIdentifier(
		X509Certificate2 certificate,
		string? expectedSki = null,
		X509Certificate2? issuerCertificate = null)
	{
		Xssert.IsNotNull(certificate);

		// If user didn't pass any expected ski, use parent aki from configuration.
		expectedSki ??= _parentAki;

		if (expectedSki.HasNothing()
			&& issuerCertificate == null
			&& _certificateSettings?.CaAuthorityKeyIdentifier == null)
		{
			throw new ArgumentNullException("ExpectedSki", "Either expected SKI, issuer certificate, or parent AKI in settings must be provided for SKI validation.");
		}

		// Find SKI extension
		var certificate_ski_extension = certificate.Extensions
			.OfType<X509SubjectKeyIdentifierExtension>()
			.FirstOrDefault();

		Xssert.IsNotNull(certificate_ski_extension, "SkiExtension");

		var certificate_ski = certificate_ski_extension.SubjectKeyIdentifier?.Replace(" ", "").ToUpperInvariant();

		// Compare with expected SKI if provided
		if (expectedSki.HasSomething())
		{
			var expected = expectedSki.Replace(" ", "").ToUpperInvariant();
			if (expected.Equals(certificate_ski, StringComparison.InvariantCultureIgnoreCase) == false)
			{
				return false;
			}
		}

		// Compare SKI with issuer’s AKI (Authority Key Identifier) if provided
		if (issuerCertificate != null)
		{
			var aki_extension = issuerCertificate.Extensions
									.OfType<X509AuthorityKeyIdentifierExtension>()
									.FirstOrDefault(); // AKI OID // e => e.Oid?.Value == "2.5.29.35"

			if (aki_extension != null)
			{
				string issuer_aki_hex = BitConverter.ToString(aki_extension.RawData).Replace("-", "");
				if (issuer_aki_hex.Equals(certificate_ski, StringComparison.InvariantCultureIgnoreCase) == false)
				{
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Tries to parse a string into a StoreLocation enum value.
	/// </summary>
	/// <param name="location"></param>
	/// <param name="storeLocation"></param>
	/// <returns></returns>
	public bool TryParseStoreLocation(string? location, out StoreLocation storeLocation)
	{
		storeLocation = StoreLocation.LocalMachine; // Default value

		if (string.IsNullOrWhiteSpace(location))
		{
			return false;
		}

		// Try direct enum parsing (case-insensitive)
		if (Enum.TryParse(location.Trim(), ignoreCase: true, out StoreLocation parsed))
		{
			storeLocation = parsed;
			return true;
		}

		// Optionally support common aliases
		switch (location.Trim().ToLowerInvariant())
		{
			case "user":
			case "current":
			case "currentuser":
				storeLocation = StoreLocation.CurrentUser;
				return true;
			case "machine":
			case "localmachine":
				storeLocation = StoreLocation.LocalMachine;
				return true;
			default:
				return false;
		}
	}

	/// <summary>
	/// Tries to parse a string into a StoreName enum value.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="storeName"></param>
	/// <returns></returns>
	public bool TryParseStoreName(string? name, out StoreName storeName)
	{
		storeName = StoreName.My; // Default value

		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}

		// Try direct enum parsing (case-insensitive)
		if (Enum.TryParse(name.Trim(), ignoreCase: true, out StoreName parsed))
		{
			storeName = parsed;
			return true;
		}

		// Support common aliases and friendly names
		switch (name.Trim().ToLowerInvariant())
		{
			case "personal":
			case "my":
				storeName = StoreName.My;
				return true;

			case "root":
			case "trustedroot":
			case "trusted":
				storeName = StoreName.Root;
				return true;

			case "ca":
			case "certificateauthority":
				storeName = StoreName.CertificateAuthority;
				return true;

			case "authroot":
			case "thirdpartyroot":
				storeName = StoreName.AuthRoot;
				return true;

			case "trustedpeople":
			case "people":
				storeName = StoreName.TrustedPeople;
				return true;

			case "trustedpublisher":
			case "publisher":
				storeName = StoreName.TrustedPublisher;
				return true;

			case "disallowed":
			case "revoked":
				storeName = StoreName.Disallowed;
				return true;

			default:
				return false;
		}
	}

	#region - Private Methods -

	/// <summary>
	/// Gets the parent certificates from the configuration settings.
	/// </summary>
	/// <param name="thumbprints"></param>
	/// <returns></returns>
	/// <exception cref="ConfigurationErrorsException"></exception>
	private List<X509Certificate2> DoGetCertificates(string[] thumbprints)
	{
		List<X509Certificate2> all_certs = new List<X509Certificate2>();

		if (thumbprints.Any(thumbPrint => thumbPrint.HasNothing()) == true)
		{
			throw new ConfigurationErrorsException($"Certificate thumbprint(s) cannot be null or empty.");
		}

		using var store = new X509Store(_storeName, _storeLocation);
		store.Open(OpenFlags.ReadOnly);

		foreach (var thumbprint in thumbprints)
		{
			if (DoTryGetCertificate(thumbprint, store, out var cert) == false)
			{
				throw new ConfigurationErrorsException($"Requested certificate with thumbprint '{thumbprint}' not found.");
			}

			if (cert != null)
			{
				all_certs.Add(cert);
			}
		}

		return all_certs;
	}

	private bool DoTryGetCertificate(string thumbprint, StoreLocation storeLocation, StoreName storeName, out X509Certificate2? x509Certificate2)
	{
		using var store = new X509Store(storeName, storeLocation);
		store.Open(OpenFlags.ReadOnly);

		return DoTryGetCertificate(thumbprint, store, out x509Certificate2);
	}

	private bool DoTryGetCertificate(string thumbprint, X509Store store, out X509Certificate2? x509Certificate2)
	{
		x509Certificate2 = null;

		var thumbprint_to_find = thumbprint.Replace(" ", "").ToUpperInvariant();

		var certs = store.Certificates.Find(
						X509FindType.FindByThumbprint,
						thumbprint_to_find,
						validOnly: false);

		if (certs.Count > 0)
		{
			x509Certificate2 = certs[0];
			return true;
		}

		return false;
	}

	#endregion - Private Methods -
}
