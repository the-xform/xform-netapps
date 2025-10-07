namespace MySampleWebApi;

public interface ISampleService
{
	void Run();
}

public class SampleService : ISampleService
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
		_logger.LogInformation("AdditionalCommandLineConfigFileKey	: " + _config["AdditionalCommandLineConfigFileKey"]);

		Console.WriteLine("MySampleSetting						: " + _config["MySampleSetting"]);
		Console.WriteLine("DbConnectionString					: " + _config["ConnectionStrings:DbConnectionString"]);
		Console.WriteLine("AdditionalCommandLineConfigFileKey	: " + _config["AdditionalCommandLineConfigFileKey"]);
	}
}	
