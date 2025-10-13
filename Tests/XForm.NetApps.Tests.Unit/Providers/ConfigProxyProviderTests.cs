using System.Configuration;
using XForm.NetApps.Providers;

namespace XForm.NetApps.Tests.Unit.Providers;

public class ConfigProxyProviderTests : IDisposable
{
	private readonly string _tempConfigPath;

	public ConfigProxyProviderTests()
	{
		// Create a temporary .config file for testing
		_tempConfigPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.config");
		File.WriteAllText(_tempConfigPath, @"
				<configuration>
					<appSettings>
						<add key='AppName' value='TestApp'/>
						<add key='MaxItems' value='42'/>
						<add key='ApiPassword' value='SuperSecret'/>
					</appSettings>
					<connectionStrings>
						<add name='DefaultConnection' connectionString='Server=.;Database=TestDb;Trusted_Connection=True;' providerName='System.Data.SqlClient'/>
					</connectionStrings>
				</configuration>");
	}

	public void Dispose()
	{
		if (File.Exists(_tempConfigPath))
			File.Delete(_tempConfigPath);
	}

	// -------------------
	// Positive Tests
	// -------------------

	[Fact]
	public void Constructor_WithValidConfigFile_ShouldLoadAppSettingsAndConnectionStrings()
	{
		var provider = new ConfigProxyProvider(_tempConfigPath);

		Assert.NotNull(provider.AppSettings);
		Assert.NotNull(provider.ConnectionStrings);
		Assert.Equal("TestApp", provider.AppSettings["AppName"]);
		//Assert.Single(provider.ConnectionStrings.Cast<ConnectionStringSettings>());

		var conn_str = provider.ConnectionStrings["DefaultConnection"];
		Assert.NotNull(conn_str);
		Assert.Equal("Server=.;Database=TestDb;Trusted_Connection=True;", conn_str.ConnectionString);
		Assert.Equal("System.Data.SqlClient", conn_str.ProviderName);
	}

	[Fact]
	public void GetAppSetting_ShouldReturnValueAndConvertType()
	{
		var provider = new ConfigProxyProvider(_tempConfigPath);

		string? appName = provider.GetAppSetting<string>("AppName");
		int maxItems = provider.GetAppSetting<int>("MaxItems");

		Assert.Equal("TestApp", appName);
		Assert.Equal(42, maxItems);
	}

	[Fact]
	public void GetAppSetting_WithDefaultValue_ShouldReturnDefaultIfKeyMissing()
	{
		var provider = new ConfigProxyProvider(_tempConfigPath);

		string? value = provider.GetAppSetting("MissingKey", "DefaultValue");

		Assert.Equal("DefaultValue", value);
	}

	[Fact]
	public void GetAllSettings_ShouldIncludeEnvironmentAndAppSettings()
	{
		// Arrange
		const string envPrefix = "APP_";
		const string envKey = "APP_SOME_SETTING";
		Environment.SetEnvironmentVariable(envKey, "FromEnv", EnvironmentVariableTarget.Machine);

		var provider = new ConfigProxyProvider(_tempConfigPath, envPrefix);

		// Act
		var all = provider.GetAllSettings();

		// Assert
		Assert.True(all.ContainsKey("Environment"));
		Assert.True(all.ContainsKey("AppSettings"));

		Assert.Contains("AppName", all["AppSettings"].Keys);
		Assert.Equal("*****", all["AppSettings"]["ApiPassword"]); //*
	}
}