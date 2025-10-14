// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Security.Cryptography.X509Certificates;

namespace XForm.NetApps.Interfaces;

public interface ICertificateProvider
{
	/// <summary>
	/// The location of the configured store.
	/// </summary>
	public StoreLocation CertStoreLocation { get; }

	/// <summary>
	/// The name of the configured store.
	/// </summary>
	public StoreName CertStoreName { get; }

	/// <summary>
	/// Gets a certificate from the loaded collection of certificates.
	/// </summary>
	/// <param name="thumbprint"></param>
	/// <returns></returns>
	/// <exception cref="KeyNotFoundException"></exception>
	X509Certificate2? GetCertificate(string thumbprint);

	/// <summary>
	/// Gets a certificate from the specified store.
	/// </summary>
	/// <param name="thumbprint"></param>
	/// <param name="storeLocation"></param>
	/// <param name="storeName"></param>
	/// <returns></returns>
	X509Certificate2? GetCertificate(string thumbprint, StoreLocation storeLocation, StoreName storeName);

	/// <summary>
	/// Add additional certificates to the existing collection of certificates.
	/// </summary>
	/// <param name="certificates"></param>
	void AddCertificates(List<X509Certificate2> certificates);

	/// <summary>
	/// Add additional certificates to the existing collection of certificates.
	/// </summary>
	/// <param name="thumbprints"></param>
	/// <param name="notFoundThumbprints"></param>
	void AddCertificates(List<string> thumbprints, out List<string> notFoundThumbprints);

	/// <summary>
	/// Validates the specified certificate against a certificate chain, with the given certificate policy. If none specified, uses the default policy.
	/// </summary>
	/// <param name="certificate">The certificate that needs to be validated.</param>
	/// <param name="chainStatus">The statuses of each certificate evaluated in the certificate chain.</param>
	/// <param name="chainPolicy">The chain policy to use.</param>
	/// <returns></returns>
	bool ValidateCertificateChain(X509Certificate2 certificate, out X509ChainStatus[] chainStatus, X509ChainPolicy? chainPolicy = null);

	/// <summary>
	/// Validates the specified certificate for expiration date.
	/// </summary>
	/// <param name="certificate"></param>
	/// <returns></returns>
	bool ValidateCertificate(X509Certificate2? certificate);

	/// <summary>
	/// Validates the Subject Key Identifier (SKI) of an X509Certificate2.
	/// </summary>
	/// <param name="certificate">The certificate to validate.</param>
	/// <param name="expectedSki">Optional expected SKI value to compare against (hex string).</param>
	/// <param name="issuerCertificate">Optional issuer certificate to compare SKI with its AKI.</param>
	/// <returns>True if SKI is valid (and matches expected/issuer if provided).</returns>
	public bool ValidateSubjectKeyIdentifier(X509Certificate2 certificate, string? expectedSki = null, X509Certificate2? issuerCertificate = null);

	/// <summary>
	/// Tries to parse a string into a StoreLocation enum value.
	/// </summary>
	/// <param name="location"></param>
	/// <param name="storeLocation"></param>
	/// <returns></returns>
	bool TryParseStoreLocation(string? location, out StoreLocation storeLocation);

	/// <summary>
	/// Tries to parse a string into a StoreName enum value.
	/// </summary>
	/// <param name="name"></param>
	/// <param name="storeName"></param>
	/// <returns></returns>
	bool TryParseStoreName(string? name, out StoreName storeName);
}