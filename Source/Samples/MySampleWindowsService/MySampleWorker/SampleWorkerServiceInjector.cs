using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XForm.NetApps.Builders.WinService.Interfaces;
using XForm.Utilities.Validations;

namespace MySampleWorker;

public class SampleWorkerServiceInjector : IWorkerServiceInjector
{
    public void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
    {
        Console.WriteLine("SampleWorkerServiceInjector.ConfigureConfiguration: Configuring additional configuration from worker assembly...");
		var config = configurationBuilder.Build();

        // Ensure that service name is specified.
        var service_name = config.GetValue<string>("ServiceName");
        Xssert.IsNotNull(service_name, nameof(service_name));

        // Ensure that app config path is specified.
        var app_config_path = config.GetValue<string>("AppConfigPath");
		Xssert.IsNotNull(app_config_path, nameof(app_config_path));
    }

    public void ConfigureServicesForWorker(IServiceCollection serviceCollection, IConfiguration workerConfigurationSection)
    {
        Console.WriteLine("SampleWorkerServiceInjector.ConfigureServicesForWorker: Configuring services from worker assembly...");
		serviceCollection.AddSingleton<ISampleServiceInWorker, SampleServiceInWorker>();

        // Add HttpClientFactory
		serviceCollection.AddHttpClient();
	}
}
