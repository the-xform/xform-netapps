using Microsoft.Extensions.Logging;
using XForm.NetApps.Builders.WinService.Interfaces;

namespace MySampleWorker;

public class SampleWorker : IWorker
{
	private readonly ILogger _logger;
	private readonly int _retryCount;
	private readonly string _defaultFirstName;

	public string Name => "Sample Worker";

	public SampleWorker(ILogger<SampleWorker> logger, int retryCount, string defaultFirstName)
	{
		_logger = logger;
		_retryCount = retryCount;
		_defaultFirstName = defaultFirstName;
	}

	public async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		await Task.CompletedTask;

		_logger.LogInformation($"{nameof(SampleWorker)}.{nameof(ExecuteAsync)}: {Name} - Time is {DateTime.Now}. RetryCount is {_retryCount}, DefaultFirstName is {_defaultFirstName}.");
		Console.WriteLine($"{nameof(SampleWorker)}.{nameof(ExecuteAsync)}: {Name} - Time is {DateTime.Now}");
	}
}
