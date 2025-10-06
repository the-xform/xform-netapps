using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MySampleWinFormsApp;

internal interface ISampleService
{
	void Run();
}

internal class SampleService : ISampleService
{
	private readonly IConfiguration _config;
	private readonly ILogger<SampleService> _logger;

	public SampleService(IConfiguration config, ILogger<SampleService> logger)
	{
		_config = config;
		_logger = logger;
	}

	public void Run()
	{
		_logger.LogInformation("MySampleSetting						: " + _config["MySampleSetting"]);
		_logger.LogInformation("DbConnectionString					: " + _config["ConnectionStrings:DbConnectionString"]);
		_logger.LogInformation("AdditionalConfigKey					: " + _config["AdditionalConfigKey"]);
	}
}	
