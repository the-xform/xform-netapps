// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Xml;
using XForm.NetApps.Interfaces;
using XForm.Utilities.Validations;

namespace XForm.NetApps.Providers
{
	/// <summary>
	/// Allows reading the settings and/or connectionstring from <application>.exe.config and system's environment settings.
	/// </summary>
	public class ConfigProxyProvider : IConfigProxyProvider
	{
		private readonly string _environmentSettingNamePrefix;
		private readonly NameValueCollection _appSettings;
		private readonly ConnectionStringSettingsCollection _connectionStrings;

		#region - Properties - 

		/// <summary>
		/// App settings read from the <application>.exe.config file.
		/// </summary>
		public NameValueCollection AppSettings => _appSettings;

		/// <summary>
		/// Gets or sets the collection of connection strings used to configure database connections.
		/// </summary>
		public ConnectionStringSettingsCollection ConnectionStrings => _connectionStrings;

		#endregion - Properties - 

		#region - Constructors - 

		/// <summary>
		/// Implementation of IConfigProxyProvider.
		/// </summary>
		public ConfigProxyProvider()
		{
			_environmentSettingNamePrefix = "";
			_appSettings = ConfigurationManager.AppSettings;
			_connectionStrings = ConfigurationManager.ConnectionStrings;
		}

		/// <summary>
		/// Implementation of IConfigProxyProvider.
		/// </summary>
		/// <param name="pathToApplicationConfigFile">Path to the <application>.exe.config file.</param>
		/// <param name="environmentSettingNamePrefix">The prefix to the names of environment variables that identifies the application specific keys. Default is empty.</param>
		/// <exception cref="ArgumentException"></exception>
		public ConfigProxyProvider(string pathToApplicationConfigFile, string environmentSettingNamePrefix = "")
		{
			if (System.IO.File.Exists(pathToApplicationConfigFile) == false)
			{
				throw new ArgumentException($"Config file not found at {pathToApplicationConfigFile}", nameof(pathToApplicationConfigFile));
			}

			_environmentSettingNamePrefix = environmentSettingNamePrefix;

			// In the following way, Configuration object merges multiple configuration sources, including:
			// Machine.config (global .NET configuration file under C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config\machine.config)
			// Other default sources, like system-level connection strings (SQLEXPRESS, etc.)
			// This will cause more conenctionstrings like SQLEXPRESS may also get loaded. 
			var config_map = new ExeConfigurationFileMap { ExeConfigFilename = pathToApplicationConfigFile };
			var config = ConfigurationManager.OpenMappedExeConfiguration(config_map, ConfigurationUserLevel.None);

			//// To ensure that no machine level conenctionstrings get loaded, do the following -
			//config.ConnectionStrings.SectionInformation.ConfigSource = pathToApplicationConfigFile;

			_appSettings = DoGetAppSettings(config);
			_connectionStrings = config.ConnectionStrings.ConnectionStrings;
		}

		#endregion - Constructors - 

		#region - Public Methods - 

		/// <summary>
		/// Gets the value of the app setting with the specified key.
		/// Settings from application configuration file takes precedence over environment variables.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException"></exception>
		/// <exception cref="InvalidCastException"></exception>
		public T? GetAppSetting<T>(string key)
		{
			T? convert_from_string;

			if (DoTryGetAppSetting(key, out convert_from_string))
			{
				return convert_from_string;
			}

			throw new KeyNotFoundException($"Setting '{key}' not found.");
		}

		/// <summary>
		/// Gets the value of the app setting with the specified key. If the key is not found, returns the specified default value.
		/// Settings from application configuration file takes precedence over environment variables.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns>T</returns>
		/// <exception cref="InvalidCastException"></exception>
		public T? GetAppSetting<T>(string key, T? defaultValue)
		{
			T? convert_from_string;

			return DoTryGetAppSetting(key, out convert_from_string) ? convert_from_string : defaultValue;
		}

		/// <summary>
		/// Returns all settings in both environment variables and app settings. 
		/// The returned dictionary contains two key-value pairs with keys "Environment" and "AppSettings". These key-value pairs contain 
		/// the key-value pairs from environment and app settings respectively.
		/// </summary>
		/// <returns><see cref="Dictionary<string, IDictionary<string, string>>"/> </returns>
		public Dictionary<string, IDictionary<string, string?>> GetAllSettings()
		{
			var dictionary_env = new Dictionary<string, string?>();
			var dictionary_app_settings = new Dictionary<string, string?>();

			// Read environment variables
			var dic_env_settings = System.Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
			foreach (DictionaryEntry entry in dic_env_settings)
			{
				var key_str = entry.Key as string;

				if (key_str.HasSomething())
				{
					Xssert.IsNotNull(key_str);
					if (key_str.StartsWith(_environmentSettingNamePrefix)) // All our custom environment variables start with ENV_
					{
						if (key_str.ToLower().Contains("pass") == true
							|| key_str.ToLower().Contains("secret") == true
							|| key_str.ToLower().Contains("key") == true)
						{
							var val = entry.Value?.ToString();

							// Hide the password if any value is present, else let it be null. This will help unhiding any config errors with passwords.
							dictionary_env[key_str] = val ?? "*****";
						}
						else
						{
							dictionary_env[key_str] = entry.Value?.ToString();
						}
					}
				}
				else
				{
					// Do not add the entry to dictionary if the key is null.
				}
			}

			// Read app settings
			foreach (var key in AppSettings.AllKeys)
			{
				if (key.HasSomething())
				{
					Xssert.IsNotNull(key);
					if (key.ToLower().Contains("pass") == true
						|| key.ToLower().Contains("secret") == true
						|| key.ToLower().Contains("key") == true)
					{
						var val = AppSettings[key];

						// Hide the password if any value is present, else let it be null. This will help unhiding any config errors with passwords.
						dictionary_app_settings[key] = (val == null) ? null : "*****";
					}
					else
					{
						dictionary_app_settings[key] = AppSettings[key];
					}
				}
				else
				{
					// Do not add the entry to dictionary if the key is null.
				}
			}

			return new Dictionary<string, IDictionary<string, string?>>()
			{
				["Environment"] = dictionary_env,
				["AppSettings"] = dictionary_app_settings
			};
		}

		#endregion - Public Methods - 

		#region - Private Methods - 

		private static NameValueCollection DoGetAppSettings(Configuration config)
		{
			var app_settings_xml = config.AppSettings?.SectionInformation?.GetRawXml();

			if (app_settings_xml != null)
			{
				var settings_xml_doc = new XmlDocument();
				settings_xml_doc.Load(new StringReader(app_settings_xml));
				var handler = new NameValueSectionHandler();
				var collection = handler.Create(null, null, settings_xml_doc.DocumentElement) as NameValueCollection;

				return collection ?? new NameValueCollection();
			}
			
			return new NameValueCollection();
		}

		private bool DoTryGetAppSetting<T>(string key, out T? convertFromString)
		{
			if (!string.IsNullOrEmpty(key))
			{
				// #11543: First try to locate the key in web.config, if it doesnt exists, then fallback to Environment variable
				var value = AppSettings[key];
				value = value ?? System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Machine);

				if (value != null)
				{
					try
					{
						var type_converter = TypeDescriptor.GetConverter(typeof(T));
						{
							convertFromString = (T?)type_converter.ConvertFromString(value);
							return (convertFromString != null);
						}
					}
					catch (Exception ex)
					{
						throw new InvalidCastException($"Could not convert value '{value}' of settings key '{key}' from type '{key.GetType().Name}' to type '{typeof(T).Name}'.", ex);
					}
				}
			}

			convertFromString = default(T);
			return false;
		}

		#endregion - Private Methods - 
	}
}