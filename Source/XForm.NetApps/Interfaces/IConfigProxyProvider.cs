// SPDX-License-Identifier: MIT
// Copyright (c) [Rohit Ahuja]
// This file is licensed under the MIT License.
// See the LICENSE file in the project root for details.

using System.Collections.Specialized;
using System.Configuration;

namespace XForm.NetApps.Interfaces;

public interface IConfigProxyProvider
{
	/// <summary>
	/// App settings read from the <application>.exe.config file.
	/// </summary>
	NameValueCollection AppSettings { get; }

	/// <summary>
	/// Gets or sets the collection of connection strings used to configure database connections.
	/// </summary>
	ConnectionStringSettingsCollection ConnectionStrings { get; }

	/// <summary>
	/// Gets the value of the app setting with the specified key.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <returns></returns>
	/// <exception cref="KeyNotFoundException"></exception>
	T? GetAppSetting<T>(string key);

	/// <summary>
	/// Gets the value of the app setting with the specified key. If the key is not found, returns the specified default value.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key"></param>
	/// <param name="defaultValue"></param>
	/// <returns>T</returns>
	T? GetAppSetting<T>(string key, T defaultValue);

	/// <summary>
	/// Returns all settings in both environment variables and app settings. 
	/// The returned dictionary contains two key-value pairs with keys "Environment" and "AppSettings". These key-value pairs contain 
	/// the key-value pairs from environment and app settings respectively.
	/// </summary>
	/// <returns><see cref="Dictionary<string, IDictionary<string, string>>"/> </returns>
	Dictionary<string, IDictionary<string, string?>> GetAllSettings();
}