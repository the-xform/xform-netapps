using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XForm.NetApps.Interfaces;

namespace MySampleConsoleApp;

internal interface ISampleService
{
	void Run();
}

internal class SampleService : ISampleService
{
	private readonly IConfiguration _config;
	private readonly ILogger<SampleService> _logger;
	private readonly IDbContextProvider _dbContext1;
	private readonly IDbContextProvider _dbContext2;

	public SampleService(IConfiguration config,
		ILogger<SampleService> logger,
		[FromKeyedServices("Db1ConnectionString")] IDbContextProvider dbContext1,
		[FromKeyedServices("Db2ConnectionString")] IDbContextProvider dbContext2)
	{
		_config = config;
		_logger = logger;

		_dbContext1 = dbContext1;
		_dbContext2 = dbContext2;
	}

	public void Run()
	{
		_logger.LogInformation($"MySampleSetting					: {_config["MySampleSetting"]}");
		_logger.LogInformation($"DbConnectionString					: {_config["ConnectionStrings:DbConnectionString"]}");
		_logger.LogInformation($"AdditionalCommandLineConfigFileKey	: {_config["AdditionalCommandLineConfigFileKey"]}");
		_logger.LogInformation($"DbContext1 ConnectionString		: {_dbContext1.Connection?.ConnectionString}");
		_logger.LogInformation($"DbContext2 ConnectionString		: {_dbContext2.Connection?.ConnectionString}");

		Console.WriteLine($"MySampleSetting							: {_config["MySampleSetting"]}");
		Console.WriteLine($"DbConnectionString						: {_config["ConnectionStrings:DbConnectionString"]}");
		Console.WriteLine($"AdditionalCommandLineConfigFileKey		: {_config["AdditionalCommandLineConfigFileKey"]}");
		Console.WriteLine($"DbContext1 ConnectionString				: {_dbContext1.Connection?.ConnectionString}");
		Console.WriteLine($"DbContext2 ConnectionString				: {_dbContext2.Connection?.ConnectionString}");
	}
}
