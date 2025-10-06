using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XForm.Core.Interfaces;
using XForm.Utilities.Validations;

namespace MySampleWindowsService;

public class ServiceHostServiceInjector : IServicesInjector
{
    public void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
    {
        var config = configurationBuilder.Build();

        // Ensure that service name is specified.
        var service_name = config.GetValue<string>("ServiceName");
        Xssert.IsNotNull(service_name, nameof(service_name));
        Console.WriteLine($"ServiceHostServiceInjector.ServiceName: {service_name}");

		// Ensure that app config path is specified.
		var app_config_path = config.GetValue<string>("AppConfigPath");
		Xssert.IsNotNull(app_config_path, nameof(app_config_path));
        Console.WriteLine($"ServiceHostServiceInjector.AppConfigPath: {app_config_path}");
	}

	public void ConfigureServices(IConfiguration configuration, IServiceCollection serviceCollection)
	{
		serviceCollection.AddHttpClient();
        Console.WriteLine($"ServiceHostServiceInjector: Added HttpClient");
	}
}
