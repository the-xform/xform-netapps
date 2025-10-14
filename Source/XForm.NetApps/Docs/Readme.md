# xform-netapps

**xform-netapps** is a C# library providing reusable building blocks and helpers for creating .NET applications of various types (Console, WinForms, Web API). It offers common services (configuration, certificate handling, GUID generation, logging, json serialization and deserialization) and app builders to streamline application startup and hosting. It's a library to accelerate development and standardize application architecture across multiple .NET app types.

------------------------------------------------------------------------

## Features

- Unified configuration support via `IConfiguration` (JSON, environment variables, `.exe.config`).  
- Default certificate chain validation with customizable behavior.  
- Reusable sequential GUID generator for database-friendly IDs.  
- Ready-to-use builders for Console, WinForms, and Web API applications.  
- Unit-tested components for certificates, configuration, and GUID generation.

------------------------------------------------------------------------


## Key Components & Features

| Component / Class | Purpose / Responsibility |
|------------------|------------------------|
| **CertificateProvider** | Load and manage X.509 certificates from stores or memory. Validate certificates, check expiration, and validate chains. |
| **ConfigProxyProvider** | Wrapper for reading configuration settings from `.exe.config`, JSON, and environment variables, with support for sensitive keys. |
| **SequentialGuidProvider** | Generates sequential GUIDs optimized for database insertion and indexing. |
| **ConsoleAppBuilder** | Helps build and configure console applications with pre-injected services like certificates and configuration. |
| **WinFormsAppBuilder** | Similar to ConsoleAppBuilder, but for WinForms applications, providing a ready-to-use host and services. |
| **WebApiBuilder** | Bootstraps Web API applications with default configuration, DI, and common services. |
| **CommonAppBuilder / Shared Logic** | Underlying shared logic used by the builders for configuring logging, DI, configuration, and certificates. |


------------------------------------------------------------------------

# CertificateProvider Class Documentation

> **Namespace:** `XForm.NetApps.Providers`  
> **Implements:** `ICertificateProvider`  
> **Purpose:** Provides functionality to retrieve, manage, and validate X.509 certificates from certificate stores or configuration settings.

## Overview

`CertificateProvider` is a configurable utility that allows applications to load and validate X.509 certificates from certificate stores (`LocalMachine` or `CurrentUser`) or from in-memory lists.  
It supports:

- Retrieving certificates by thumbprint  
- Validating certificate chains  
- Validating expiration dates  
- Validating Subject Key Identifiers (SKI)  
- Managing multiple certificate stores and configurations  

## Constructors

### `CertificateProvider(IConfiguration configuration)`

Initializes a new instance using configuration from an appsettings file or any `IConfiguration` source.

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var provider = new CertificateProvider(configuration);
```

**appsettings.json Example:**
```json
{
  "CertificateSettings": {
    "IsEnabled": true,
    "CertStoreLocation": "LocalMachine",
    "CertStoreName": "My",
    "Thumbprints": [
      "A1B2C3D4E5F6A7B8C9D0E1F2A3B4C5D6E7F8A9B0"
    ],
    "CaAuthorityKeyIdentifier": "123456ABCDEF"
  }
}
```

If the section is missing or invalid, a `ConfigurationErrorsException` will be thrown.

---

### `CertificateProvider(CertificateSettings certificateSettings)`

Creates the provider from a `CertificateSettings` object.

```csharp
var settings = new CertificateSettings
{
    IsEnabled = true,
    CertStoreLocation = "LocalMachine",
    CertStoreName = "My",
    Thumbprints = new[] { "A1B2C3..." }
};

var provider = new CertificateProvider(settings);
```

---

### `CertificateProvider(string[]? thumbprints, string? storeLocation = null, string? storeName = null, string? parentAki = null, bool isEnabled = true)`

Manually specifies certificate store parameters.

```csharp
var provider = new CertificateProvider(
    thumbprints: new[] { "A1B2C3..." },
    storeLocation: "CurrentUser",
    storeName: "My"
);
```

---

### `CertificateProvider(List<X509Certificate2> certificates)`

Initialize directly with a list of certificates (for in-memory scenarios).

```csharp
var certs = new List<X509Certificate2> { rootCert, clientCert };
var provider = new CertificateProvider(certs);
```

---

## Properties

### `StoreLocation CertStoreLocation`
The configured store location (`LocalMachine` or `CurrentUser`).

### `StoreName CertStoreName`
The configured store name (e.g., `My`, `Root`, `CertificateAuthority`).

### `X509ChainPolicy DefaultChainPolicy`
Default chain policy used for certificate validation:
- `RevocationMode = NoCheck`
- `VerificationFlags = NoFlag`
- `VerificationTimeIgnored = false`

---

## Public Methods

### `GetCertificate(string thumbprint)`

Retrieves a certificate from the provider’s in-memory collection by its thumbprint.

```csharp
var cert = provider.GetCertificate("A1B2C3D4...");
if (cert != null)
{
    Console.WriteLine($"Found certificate: {cert.Subject}");
}
```

Returns `null` if not found.

---

### `GetCertificate(string thumbprint, StoreLocation storeLocation, StoreName storeName)`

Fetches a certificate directly from a specified store.

```csharp
var cert = provider.GetCertificate(
    "A1B2C3D4...",
    StoreLocation.LocalMachine,
    StoreName.My
);
```

---

### `AddCertificates(List<string> thumbprints, out List<string> notFoundThumbprints)`

Adds certificates by thumbprints from the configured store and appends them to the internal list.  
Returns thumbprints that were not found.

```csharp
var thumbprints = new List<string> { "A1B2C3...", "XYZ123..." };

provider.AddCertificates(thumbprints, out var notFound);

if (notFound.Any())
{
    Console.WriteLine($"Missing certs: {string.Join(", ", notFound)}");
}
```

---

### `AddCertificates(List<X509Certificate2> certificates)`

Adds a list of certificate objects to the existing collection.

```csharp
var extraCerts = new List<X509Certificate2> { newCert1, newCert2 };
provider.AddCertificates(extraCerts);
```

---

### `ValidateCertificateChain(X509Certificate2 certificate, out X509ChainStatus[] chainStatus, X509ChainPolicy? chainPolicy = null)`

Validates a certificate chain using the configured certificates as potential issuers.

```csharp
bool isValid = provider.ValidateCertificateChain(clientCert, out var chainStatus);

Console.WriteLine($"Valid chain: {isValid}");
foreach (var status in chainStatus)
{
    Console.WriteLine($"Status: {status.StatusInformation}");
}
```

**Example with a custom chain policy:**

```csharp
var policy = new X509ChainPolicy
{
    RevocationMode = X509RevocationMode.NoCheck,
    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
};

bool isValid = provider.ValidateCertificateChain(clientCert, out var status, policy);
```

If the provider is disabled, it throws `InvalidOperationException`.

---

### `ValidateCertificate(X509Certificate2? certificate)`

Checks if a certificate is currently valid (not expired or not yet valid).

```csharp
bool isValid = provider.ValidateCertificate(cert);
Console.WriteLine(isValid ? "Certificate is valid." : "Certificate is invalid or expired.");
```

If the provider is disabled, throws `InvalidOperationException`.

---

### `ValidateSubjectKeyIdentifier(X509Certificate2 certificate, string? expectedSki = null, X509Certificate2? issuerCertificate = null)`

Validates a certificate’s Subject Key Identifier (SKI) against an expected SKI, the issuer’s Authority Key Identifier (AKI), or the configured parent AKI.

```csharp
bool skiValid = provider.ValidateSubjectKeyIdentifier(clientCert, expectedSki: "ABCD1234EF...");
Console.WriteLine($"SKI valid: {skiValid}");
```

Or using issuer certificate:

```csharp
bool skiMatchesIssuer = provider.ValidateSubjectKeyIdentifier(clientCert, issuerCertificate: caCert);
```

If no SKI/AKI reference is provided (and none exists in config), an `ArgumentNullException` will be thrown.

---

### `TryParseStoreLocation(string? location, out StoreLocation storeLocation)`

Parses a string into a `StoreLocation` enum.

```csharp
if (provider.TryParseStoreLocation("CurrentUser", out var loc))
{
    Console.WriteLine($"Parsed store location: {loc}");
}
```

Supported values and aliases:
- `CurrentUser` — aliases: `"user"`, `"current"`, `"currentuser"`
- `LocalMachine` — aliases: `"machine"`, `"localmachine"`

Returns `true` when parsing succeeds.

---

### `TryParseStoreName(string? name, out StoreName storeName)`

Parses a string into a `StoreName` enum.

```csharp
if (provider.TryParseStoreName("Root", out var store))
{
    Console.WriteLine($"Parsed store name: {store}");
}
```

Supported values and aliases:
- `My` — aliases: `"personal"`, `"my"`
- `Root` — aliases: `"root"`, `"trustedroot"`, `"trusted"`
- `CertificateAuthority` — aliases: `"ca"`, `"certificateauthority"`
- `AuthRoot` — aliases: `"authroot"`, `"thirdpartyroot"`
- `TrustedPeople` — aliases: `"trustedpeople"`, `"people"`
- `TrustedPublisher` — aliases: `"trustedpublisher"`, `"publisher"`
- `Disallowed` — aliases: `"disallowed"`, `"revoked"`

Returns `true` when parsing succeeds.

---

## Error Handling

| Exception Type | Scenario |
|----------------|-----------|
| `ConfigurationErrorsException` | Missing or invalid configuration section or thumbprint |
| `ArgumentNullException` | Null argument to validation method |
| `InvalidOperationException` | Provider is disabled when a validation or retrieval method is called |

---

## Example: Full Usage

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var provider = new CertificateProvider(configuration);

// Get a certificate
var cert = provider.GetCertificate("A1B2C3D4...");

// Validate it
if (provider.ValidateCertificate(cert))
{
    Console.WriteLine("Certificate is within its validity period.");
}

// Validate chain
bool validChain = provider.ValidateCertificateChain(cert, out var chainStatus);
Console.WriteLine($"Chain valid: {validChain}");
```

---

## Notes

- The provider does **not** automatically trust self-signed certificates unless present in the configured store or passed into the provider's certificate list.
- The `DefaultChainPolicy` avoids revocation checks by default to simplify usage in test and development environments.
- Thumbprints are case-insensitive and can include or omit spaces.
- Use `AddCertificates` with `notFoundThumbprints` to detect missing certificates at runtime.

---




# ConfigProxyProvider Class Documentation

`ConfigProxyProvider` allows reading application settings and connection strings from the application's `.exe.config` file and system environment variables. Settings from the application configuration file take precedence over environment variables.

## Table of Contents

* [Properties](#properties)
* [Constructors](#constructors)
* [Public Methods](#public-methods)

  * [GetAppSetting<T>(string key)](#getappsettingtstring-key)
  * [GetAppSetting<T>(string key, T? defaultValue)](#getappsettingtstring-key-t-defaultvalue)
  * [GetAllSettings()](#getallsettings)
* [Examples](#examples)

---

## Properties

### `AppSettings`

```csharp
public NameValueCollection AppSettings { get; }
```

Returns the application settings read from the `.exe.config` file.

### `ConnectionStrings`

```csharp
public ConnectionStringSettingsCollection ConnectionStrings { get; }
```

Returns the collection of connection strings used to configure database connections.


## Constructors

### `ConfigProxyProvider()`

```csharp
var provider = new ConfigProxyProvider();
```

Initializes the provider using the default application configuration file and system environment variables.

---

### `ConfigProxyProvider(string pathToApplicationConfigFile, string environmentSettingNamePrefix = "")`

```csharp
var provider = new ConfigProxyProvider("C:\\MyApp\\MyApp.exe.config", "ENV_");
```

* `pathToApplicationConfigFile` – Path to the application's `.exe.config` file.
* `environmentSettingNamePrefix` – Optional prefix for environment variables to filter only application-specific keys.

Throws `ArgumentException` if the file does not exist.

> **Note:** If you use the default configuration, system-level connection strings (like `SQLEXPRESS`) may also be loaded.

---

## Public Methods

### `GetAppSetting<T>(string key)`

```csharp
public T? GetAppSetting<T>(string key)
```

Gets the value of the app setting with the specified key. Throws `KeyNotFoundException` if the key is not found. Throws `InvalidCastException` if conversion fails.

**Example:**

```csharp
string appName = provider.GetAppSetting<string>("AppName");
int retryCount = provider.GetAppSetting<int>("RetryCount");
```

---

### `GetAppSetting<T>(string key, T? defaultValue)`

```csharp
public T? GetAppSetting<T>(string key, T? defaultValue)
```

Gets the value of the app setting with the specified key. Returns `defaultValue` if the key is not found.

**Example:**

```csharp
int retryCount = provider.GetAppSetting<int>("RetryCount", 3); // returns 3 if key not found
string logPath = provider.GetAppSetting<string>("LogPath", "C:\\Logs");
```

---

### `GetAllSettings()`

```csharp
public Dictionary<string, IDictionary<string, string?>> GetAllSettings()
```

Returns all settings from both environment variables and application settings.
The returned dictionary contains two entries:

* `"Environment"` – filtered environment variables with prefix (if provided)
* `"AppSettings"` – application settings

Sensitive keys containing `"pass"`, `"secret"`, or `"key"` are masked.

**Example:**

```csharp
var allSettings = provider.GetAllSettings();

foreach (var envSetting in allSettings["Environment"])
{
    Console.WriteLine($"{envSetting.Key} = {envSetting.Value}");
}

foreach (var appSetting in allSettings["AppSettings"])
{
    Console.WriteLine($"{appSetting.Key} = {appSetting.Value}");
}
```

---

## Examples

### Initialize default provider

```csharp
var provider = new ConfigProxyProvider();

// Read an application setting
string appName = provider.GetAppSetting<string>("AppName");

// Read a setting with default value
int maxRetries = provider.GetAppSetting<int>("MaxRetries", 5);

// Read all settings
var settings = provider.GetAllSettings();
```

### Initialize with a specific config file

```csharp
var provider = new ConfigProxyProvider(@"C:\\MyApp\\MyApp.exe.config", "ENV_");

// Read app setting
string connectionString = provider.GetAppSetting<string>("DefaultConnection");

// Get all environment-specific and app settings
var allSettings = provider.GetAllSettings();
```

### Handling missing keys

```csharp
try
{
    string missingSetting = provider.GetAppSetting<string>("MissingKey");
}
catch (KeyNotFoundException ex)
{
    Console.WriteLine("Setting not found: " + ex.Message);
}

// Using default value instead
string value = provider.GetAppSetting<string>("MissingKey", "default-value");
```

---

**Note:**

* Application settings take precedence over environment variables.
* Sensitive keys are masked automatically in `GetAllSettings()`.
* The class implements `IConfigProxyProvider` and can be used wherever dependency injection is supported.


---





# SequentialGuidProvider Class Documentation

**Namespace:** `XForm.NetApps.Providers`  
**Implements:** `ISequentialGuidProvider`  
**License:** MIT  


## Overview

`SequentialGuidProvider` generates sequential GUIDs optimized for databases and indexing. Using sequential GUIDs instead of purely random GUIDs can reduce fragmentation in indexes and improve insert performance in database systems like SQL Server.


## Public Methods

### `Guid NewGuid(SequentialGuidType guidType = SequentialGuidType.SequentialAtEndFromGuid)`

Generates a new sequential GUID.

**Parameters:**

- `guidType` (optional): Specifies the type of sequential GUID to generate. Default is `SequentialAtEndFromGuid`.

**Returns:**  
`Guid` – A newly generated sequential GUID.

**Example:**

```csharp
var provider = new SequentialGuidProvider();

// Default sequential GUID (SQL Server optimized)
Guid defaultGuid = provider.NewGuid();
Console.WriteLine(defaultGuid);

// Sequential GUID as string
Guid stringGuid = provider.NewGuid(SequentialGuidType.SequentialAsString);
Console.WriteLine(stringGuid);

// Sequential GUID as binary
Guid binaryGuid = provider.NewGuid(SequentialGuidType.SequentialAsBinary);
Console.WriteLine(binaryGuid);
```

---




# ConsoleAppBuilder Class Documentation

**Namespace:** `XForm.NetApps.Builders.Console`  


## Overview

`ConsoleAppBuilder` provides helper methods to build and configure a console application host with commonly injected services. It simplifies creating `IHostBuilder` and `HostApplicationBuilder` instances for console applications. The common services that are automatically injected are ISequentialGuidProvider, IJsonUtilities, IConfigProxyProvider, and ICertificateProvider.


## Public Methods

### `IHostBuilder CreateHostBuilder(ConsoleAppOptions appOptions)`

Builds and returns an `IHostBuilder` for a console application with injected common services.

**Parameters:**

- `appOptions` – A `ConsoleAppOptions` object containing:
  - `AppName` (string): Name of the console application. Required.
  - `Args` (string[]): Command-line arguments.

**Returns:**  
`IHostBuilder` – Configured host builder.

**appsettings.json Example:**
```json
{
  "ApplicationName": "Sample Console Application",
  "Description": "Description of the app",

  "DOTNET_ENVIRONMENT": "development",

  // Path of the app.exe.config or app.dll.config to load the legacy configuration from. This path must be either absolute or relative to the exe/dll to which it belongs.
  "AppConfigPath": "MySampleConsoleApp.dll.config", // Path to app.config of the worker, either relative to dll containing CommonServicesInjector, or the absolute path. The folder structure is created during the packaging of windows service on build server.

  // AssemblyProvider configuration is used to load any dependencies (assemblies) at the startup of the host application.
  "AssemblyProvider": {
    "FileProviders": [
      {
        "Key": "PhysicalAssemblies",
        "IsWritable": false,

        // File provider factory that will load the assemblies from a physical folder.
        // This type must implement the interface XForm.Core.Interfaces.IFileProviderFactory.
        "Factory": "XForm.NetApps.Providers.File.PhysicalFileProviderFactory",

        // The default root folder for all assemblies to be loaded from.
        // IUse value "BASE_DIRECTORY" to indicate the base directory of the host application.
        "Root": "BASE_DIRECTORY"
      }
    ]
  },

  // ServiceInjector - Injects the common services, needed by the application, at the host level. The specified type must implement the interface XForm.Core.Interfaces.IServicesInjector.
  "ServiceInjector": {
    "IsEnabled": true,
    "AssemblyPath": "MySampleConsoleApp.dll", // Path of assembly containing the service injector for the email worker. The folder structure is created during the packaging of windows service on build server.
    "TypeName": "MySampleConsoleApp.SampleExternalServiceInjector"
  },

  // Logger
  "SeriLog": {
    "Using": [ "Serilog.Sinks.File", "Serilog.Sinks.Console" ],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "..\\Logs\\SampleApplicationLog..log", // Two '.' have been intentionally added before extension. This helps Serilog to put date in the middle.
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:o} | {Level:u1} | {ThreadId} | {ThreadName} | {Message:l} | {Properties:jl} | {Exception} | ***** End-of-Log-Entry *****{NewLine}",
          "retainedFileCountLimit": 10
        }
      }
    ]
  },

  // Any additional config files to be loaded. The path is relative to the base directory of the host application.
  // This is commented out to demonstrate loading config through command-line args instead of static config file.
  //"AdditionalJsonConfigs": [
  //  "additional-command-args-settings.json"
  //],

  "CertificateSettings": {
    "IsEnabled": false,
    "CertStoreLocation": "LocalMachine",
    "CertStoreName": "My",
    "Thumbprints": [
      // Add thumbrpints of all CA and Root certs to validate the cert chain of incoming clinet certificates
    ]
  },

  "MySampleSetting": "This is a sample setting",

  // ConnectionStrings
  "ConnectionStrings": {
    "DbConnectionString": "<%DB_CONNECTION_STRING%>"
  }
}
```

**Example:**
```csharp
using XForm.NetApps.Builders.Console;
using Microsoft.Extensions.Hosting;

var appOptions = new ConsoleAppOptions
{
    AppName = "MyConsoleApp",
    Args = args
};

IHostBuilder hostBuilder = ConsoleAppBuilder.CreateHostBuilder(appOptions);
hostBuilder.Build().Run();
```

---




# WinFormsAppBuilder Class Documentation

**Namespace:** `XForm.NetApps.Builders.WinForms`


## Overview

`WinFormsAppBuilder` provides helper methods to create and configure a host for a WinForms application with common services already injected. It simplifies creating `IHostBuilder` and `HostApplicationBuilder` instances for WinForms applications. The common services that are automatically injected are ISequentialGuidProvider, IJsonUtilities, IConfigProxyProvider, and ICertificateProvider.


**appsettings.json Example:**
```json
{
  "ApplicationName": "Sample WinForms Application",
  "Description": "Description of the app",

  "DOTNET_ENVIRONMENT": "development",

  // Path of the app.exe.config or app.dll.config to load the legacy configuration from. This path must be either absolute or relative to the exe/dll to which it belongs.
  "AppConfigPath": "MySampleWinFormsApp.dll.config", // Path to app.config of the worker, either relative to dll containing CommonServicesInjector, or the absolute path. The folder structure is created during the packaging of windows service on build server.

  // AssemblyProvider configuration is used to load any dependencies (assemblies) at the startup of the host application.
  "AssemblyProvider": {
    "FileProviders": [
      {
        "Key": "PhysicalAssemblies",
        "IsWritable": false,

        // File provider factory that will load the assemblies from a physical folder.
        // This type must implement the interface XForm.Core.Interfaces.IFileProviderFactory.
        "Factory": "XForm.NetApps.Providers.File.PhysicalFileProviderFactory",

        // The default root folder for all assemblies to be loaded from.
        // IUse value "BASE_DIRECTORY" to indicate the base directory of the host application.
        "Root": "BASE_DIRECTORY"
      }
    ]
  },

  // ServiceInjector - Injects the common services, needed by the application, at the host level. The specified type must implement the interface XForm.Core.Interfaces.IServicesInjector.
  "ServiceInjector": {
    "IsEnabled": true,
    "AssemblyPath": "MySampleWinFormsApp.dll", // Path of assembly containing the service injector for the email worker, relative to service host. The folder structure is created during the packaging of windows service on build server.
    "TypeName": "MySampleWinFormsApp.SampleExternalServiceInjector"
  },

  // Logger
  "SeriLog": {
    "Using": [ "Serilog.Sinks.File", "Serilog.Sinks.Console" ],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "..\\Logs\\SampleApplicationLog..log", // Two '.' have been intentionally added before extension. This helps Serilog to put date in the middle.
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:o} | {Level:u1} | {ThreadId} | {ThreadName} | {Message:l} | {Properties:jl} | {Exception} | ***** End-of-Log-Entry *****{NewLine}",
          "retainedFileCountLimit": 10
        }
      }
    ]
  },

  // Any additional config files to be loaded. The path is relative to the base directory of the host application.
  "AdditionalJsonConfigs": [
    "additional-config-settings.json"
  ],

  "CertificateSettings": {
    "IsEnabled": true,
    "CertStoreLocation": "LocalMachine",
    "CertStoreName": "My",
    "Thumbprints": [
      // Add thumbrpints of all CA and Root certs to validate the cert chain of incoming clinet certificates
    ]
  },

  "MySampleSetting": "This is a sample setting",

  // ConnectionStrings
  "ConnectionStrings": {
    "DbConnectionString": "<%DB_CONNECTION_STRING%>"
  }
}

```

## Public Methods

### `IHostBuilder CreateHostBuilder(WinFormsAppOptions appOptions)`

Builds and returns an `IHostBuilder` for a WinForms application with injected common services.

**Parameters:**

* `appOptions` – A `WinFormsAppOptions` object containing:

  * `AppName` (string): Name of the WinForms application. Required.
  * `Args` (string[]): Command-line arguments.

**Returns:**
`IHostBuilder` – Configured host builder.


**Example:**
```csharp
using XForm.NetApps.Builders.WinForms;
using Microsoft.Extensions.Hosting;

var appOptions = new WinFormsAppOptions
{
    AppName = "MyWinFormsApp",
    Args = args
};

IHostBuilder hostBuilder = WinFormsAppBuilder.CreateHostBuilder(appOptions);

hostBuilder.Build().Run();
```

---

# `HostApplicationBuilder CreateAppHostBuilder(WinFormsAppOptions appOptions)`

Builds and returns a `HostApplicationBuilder` for a WinForms application with injected common services.

**Parameters:**

* `appOptions` – A `WinFormsAppOptions` object containing:

  * `AppName` (string): Name of the WinForms application. Required.
  * `Args` (string[]): Command-line arguments.

**Returns:**
`HostApplicationBuilder` – Configured host application builder.


**Example:**
```csharp
using XForm.NetApps.Builders.WinForms;
using Microsoft.Extensions.Hosting;

var appOptions = new WinFormsAppOptions
{
    AppName = "MyWinFormsApp",
    Args = args
};

HostApplicationBuilder builder = WinFormsAppBuilder.CreateAppHostBuilder(appOptions);

// Example: Register services for dependency injection
builder.Services.AddSingleton<MyService>();

var app = builder.Build();
app.Run();
```

---

## Usage Notes

* `WinFormsAppBuilder` relies on `CommonAppBuilder` to configure shared services for the application.
* The `AppName` property in `WinFormsAppOptions` must be provided; otherwise, an exception is thrown using `Xssert`.
* The returned host builders can be further configured with additional services, logging, or hosted services as needed.

---




# WebApiBuilder Class Documentation

**Namespace:** `XForm.NetApps.Builders.WebApi`
**Purpose:** Creates and returns a `HostApplicationBuilder` or `IHostBuilder` for a WebApi application.


## Overview

The `WebApiBuilder` class provides methods to create and configure a host builder for Web API applications. It allows you to initialize the application with common services and custom settings using `WebApiOptions`.

This class is **static** and contains only public methods.


**appsettings.json Example:**
```json
{
  "AllowedHosts": "*",

  "ApplicationName": "Sample WebApi Application",
  "Description": "Description of the app",

  "DOTNET_ENVIRONMENT": "development",

  // Path of the app.exe.config or app.dll.config to load the legacy configuration from. This path must be either absolute or relative to the exe/dll to which it belongs.
  "AppConfigPath": "MySampleWebApi.dll.config", // Path to app.config of the worker, either relative to dll containing CommonServicesInjector, or the absolute path. The folder structure is created during the packaging of windows service on build server.

  // AssemblyProvider configuration is used to load any dependencies (assemblies) at the startup of the host application.
  "AssemblyProvider": {
    "FileProviders": [
      {
        "Key": "PhysicalAssemblies",
        "IsWritable": false,

        // File provider factory that will load the assemblies from a physical folder.
        // This type must implement the interface XForm.Core.Interfaces.IFileProviderFactory.
        "Factory": "XForm.NetApps.Providers.File.PhysicalFileProviderFactory",

        // The default root folder for all assemblies to be loaded from.
        // IUse value "BASE_DIRECTORY" to indicate the base directory of the host application.
        "Root": "BASE_DIRECTORY"
      }
    ]
  },

  // ServiceInjector - Injects the common services, needed by the application, at the host level. The specified type must implement the interface XForm.Core.Interfaces.IServicesInjector.
  "ServiceInjector": {
    "IsEnabled": true,
    "AssemblyPath": "MySampleWebApi.dll", // Path of assembly containing the service injector for the email worker, relative to service host. The folder structure is created during the packaging of windows service on build server.
    "TypeName": "MySampleWebApi.SampleExternalServiceInjector"
  },

  // Logger
  "SeriLog": {
    "Using": [ "Serilog.Sinks.File", "Serilog.Sinks.Console" ],
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "Logs\\SampleWebApiLog..log", // Two '.' have been intentionally added before extension. This helps Serilog to put date in the middle.
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:o} | {Level:u1} | {ThreadId} | {ThreadName} | {Message:l} | {Properties:jl} | {Exception} | ***** End-of-Log-Entry *****{NewLine}",
          "retainedFileCountLimit": 10
        }
      }
    ]
  },

  // Any additional config files to be loaded. The path is relative to the base directory of the host application.
  // This is commented out to demonstrate loading config through command-line args instead of static config file.
  "AdditionalJsonConfigs": [
    "additional-web-api-settings.json"
  ],

  "CertificateSettings": {
    "IsEnabled": true,
    "CertStoreLocation": "LocalMachine",
    "CertStoreName": "My",
    "Thumbprints": [
      // Add thumbrpints of all CA and Root certs to validate the cert chain of incoming clinet certificates
    ]
  },

  "MySampleSetting": "This is a sample setting",

  // ConnectionStrings
  "ConnectionStrings": {
    "DbConnectionString": "<%DB_CONNECTION_STRING%>"
  }
}
```


## Public Methods

### 1. `CreateHostBuilder(WebApiOptions webApiOptions)`

**Description:**
Builds and returns an `IHostBuilder` for a WebApi application with injected common services.

**Parameters:**

* `webApiOptions` (`WebApiOptions`): Options including API name and arguments.

**Returns:**
`IHostBuilder` – Configured host builder for the WebApi application.

**Exceptions:**

* Throws `ArgumentNullException` if `webApiOptions.ApiName` is null.

**Example:**

```csharp
using XForm.NetApps.Builders.WebApi;
using Microsoft.Extensions.Hosting;

var options = new WebApiOptions
{
    ApiName = "MyWebApi",
    Args = args
};

IHostBuilder hostBuilder = WebApiBuilder.CreateHostBuilder(options);

hostBuilder.Build().Run();
```

---

### 2. `CreateWebApplicationBuilder(WebApiOptions webApiOptions)`

**Description:**
Builds and returns a `WebApplicationBuilder` for a WebApi application. This method is suitable for minimal APIs or ASP.NET Core 6+ Web API projects.

**Parameters:**

* `webApiOptions` (`WebApiOptions`): Options including API name and arguments.

**Returns:**
`WebApplicationBuilder` – Configured web application builder for WebApi.

**Exceptions:**

* Throws `ArgumentNullException` if `webApiOptions.ApiName` is null.

**Example:**
```csharp
using XForm.NetApps.Builders.WebApi;
using Microsoft.AspNetCore.Builder;

var options = new WebApiOptions
{
    ApiName = "MyWebApi",
    Args = args
};

WebApplicationBuilder builder = WebApiBuilder.CreateWebApplicationBuilder(options);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

---

## Usage Notes

* Always ensure `WebApiOptions.ApiName` is not null; otherwise, an exception will be thrown.
* Use `CreateHostBuilder` for generic host scenarios.
* Use `CreateWebApplicationBuilder` when targeting `WebApplication` and minimal APIs.

---

## Related Classes

* `WebApiOptions`: Holds configuration options for the WebApi builder including API name and command-line arguments.
* `CommonAppBuilder`: Provides shared logic for creating host builders across different app types.
* `HostApplicationBuilderSettings`: Settings used when configuring the application builder.

---




# SqlDbContextProvider

**Namespace:** `XForm.NetApps.Providers`\
**Implements:** `IDbContextProvider`\
**License:** MIT\
**Author:** Rohit Ahuja

------------------------------------------------------------------------

## Overview

`SqlDbContextProvider` is a lightweight, disposable provider class for
managing SQL Server database connections and transactions.\
It encapsulates a `SqlConnection` and exposes methods to open/close
connections, begin/commit/rollback transactions, and ensure proper
disposal.

This class is intended to be injected or used as a scoped database
context within .NET applications.

------------------------------------------------------------------------

## Constructor

``` csharp
public SqlDbContextProvider(string name, string connectionString)
```

**Parameters** \| Name \| Type \| Description \|
\|------\|------\|-------------\| \| `name` \| `string` \| A logical
name identifying this context instance. \| \| `connectionString` \|
`string` \| The connection string used to create the underlying
`SqlConnection`. \|

------------------------------------------------------------------------

## Properties

### `string Name`
Gets the context name assigned at initialization.

### `IDbConnection? Connection`
The underlying SQL connection.

### `bool IsInTransaction`
Returns `true` if a transaction is active.

### `IDbTransaction? CurrentTransaction`
The active transaction object, if any.

------------------------------------------------------------------------

## Methods

### `OpenConnection()`

Opens the underlying SQL connection if it is closed.

### `CloseConnection()`

Closes the connection if it is open.

### `BeginTransaction()`

Begins a new SQL transaction. Throws if a transaction is already in
progress or if the connection is uninitialized.

### `CommitTransaction()`

Commits the current transaction, if active.

### `RollbackTransaction()`

Rolls back the current transaction, if active.

### `Dispose()`

Releases all managed resources, rolls back any pending transactions, and
disposes the connection.

------------------------------------------------------------------------

## Usage Example

Adding named context providers in **.NET 8** using keyed
services and then injecting them in the services/controllers.

``` csharp
builder.Services.AddKeyedSingleton<IDbContextProvider>("AppDb", (sp, key) =>
    new SqlDbContextProvider("AppDb", builder.Configuration.GetConnectionString("AppDb"))
);

builder.Services.AddKeyedSingleton<IDbContextProvider>("LogsDb", (sp, key) =>
    new SqlDbContextProvider("LogsDb", builder.Configuration.GetConnectionString("LogsDb"))
);

// Example usage in a controller
public class UserController
{
    private readonly IDbContextProvider _db;
    private readonly IDbContextProvider _logsDb;

    public UserController([FromKeyedServices("AppDb")] IDbContextProvider db, [FromKeyedServices("LogsDb")] IDbContextProvider logsDb)
    {
        _db = db;
        _logsDb = logsDb;
    }
}
```

------------------------------------------------------------------------





# License

MIT License. See the LICENSE file in the project root for details.

------------------------------------------------------------------------




# Version History

## Next
- Added IDbContext to common services that are automatically injected in the host based on new configuration setting 'SqlConnectionSettings'.
- 
## 1.1.0
- Added ICertificateProvider implementation and added it into auto-injected core implementations in ConsoleAppBuilder.CreateAppHostBuilder, WinFormsAppBuilder.CreateAppHostBuilder, and WebApiBuilder.CreateWebApplicationBuilder implementations.
- Added unit tests around CertificateProvider, ConfigProxyProvider, and SequentialGuidProvider implementations.
- Added readme with sample usage.

## 1.0.2
- Removed unused code, reduced redundancy.

## 1.0.1
- Refactored code to follow the same IHostBuilder pattern for configuring the host for all kinds of applications except Windows Services.

## 1.0.0
- Initial commit for the desired functionality in library.

------------------------------------------------------------------------