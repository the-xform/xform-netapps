namespace XForm.NetApps.ConfigurationSettings;

public class CertificatesSettings
{
	/// <summary>
	/// When true, enables the use of a certificate provider to load and validate certificates based on thumbprints.
	/// </summary>
	public bool IsEnabled { get; set; }

	/// <summary>
	/// Name of the certificate store location to load certificates from. Possible values are "CurrentUser" or "LocalMachine". Default is "LocalMachine".
	/// </summary>
	public string? StoreLocation { get; set; } = "LocalMachine";

	/// <summary>
	/// Name of the certificate store to load certificates from. Possible values are "My", "Root", "CA", "AuthRoot", "TrustedPeople", or "TrustedPublisher". Default is "My".
	/// </summary>
	public string? StoreName { get; set; } = "My";

	/// <summary>
	/// List of certificate thumbprints to load from the certificate store.
	/// </summary>
	public required string[] Thumbprints { get; set; }

	/// <summary>
	/// Authority Key Identifier (SKI) value to validate the loaded certificate's SKI against.
	/// </summary>
	public string? CaAuthorityKeyIdentifier { get; set; }
}
