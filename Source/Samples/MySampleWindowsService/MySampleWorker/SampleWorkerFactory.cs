using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MySampleWorker.Config;
using XForm.NetApps.Builders.WinService.Interfaces;
using XForm.Utilities.Validations;

namespace MySampleWorker;

/// <summary>
/// Factory class to create a worker. The type for this class is specified in the worker configuration json.
/// </summary>
public class SampleWorkerFactory : IWorkerFactory
{
	private readonly ILogger<SampleWorkerFactory> _logger;
	private readonly IHostEnvironment _hostingEnvironment;

	public SampleWorkerFactory(ILogger<SampleWorkerFactory> logger,
		IHostEnvironment hostingEnvironment
		)
	{
		_logger = logger;
		_hostingEnvironment = hostingEnvironment;

		_logger.LogInformation($"SampleWorkerFactory created in environment: {_hostingEnvironment.EnvironmentName}");
		Console.WriteLine($"SampleWorkerFactory created in environment: {_hostingEnvironment.EnvironmentName}");
	}

	public IWorker Create(IConfiguration workerConfiguration, IServiceProvider serviceProvider)
	{
		Console.WriteLine("SampleWorkerFactory.Create: Creating worker...");

		var worker_settings = workerConfiguration.Get<SampleWorkerSettings>();
		Xssert.IsNotNull(worker_settings, nameof(worker_settings));

		// Instantiate and return the worker
		return new SampleWorker(serviceProvider.GetRequiredService<ILogger<SampleWorker>>(), worker_settings.RetryCount, worker_settings.DefaultFirstName ?? "TheWorkerName");
	}
}