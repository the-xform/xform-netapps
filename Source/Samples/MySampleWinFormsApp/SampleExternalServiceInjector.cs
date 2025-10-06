using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XForm.Core.Interfaces;

namespace MySampleWinFormsApp;

internal class SampleExternalServiceInjector : IServicesInjector
{
	/// <summary>
	/// Inject any services used by the application here.
	/// </summary>
	/// <param name="configuration"></param>
	/// <param name="serviceCollection"></param>
	public void ConfigureServices(IConfiguration configuration, IServiceCollection serviceCollection)
	{
		serviceCollection.AddSingleton<ISampleService, SampleService>();
		serviceCollection.AddTransient<Form1>();
	}

	/// <summary>
	/// Inject any additional app-specific configuration here.
	/// </summary>
	/// <param name="configurationBuilder"></param>
	public void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
	{
		// Any configurations can be added here if needed.
	}
}
