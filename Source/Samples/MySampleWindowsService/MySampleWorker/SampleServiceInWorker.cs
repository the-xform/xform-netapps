using Microsoft.Extensions.Logging;

namespace MySampleWorker;

public interface ISampleServiceInWorker
{
	void DoSomething();
}

public class SampleServiceInWorker : ISampleServiceInWorker
{
	private readonly ILogger<SampleServiceInWorker> _logger;

	public SampleServiceInWorker(ILogger<SampleServiceInWorker> logger)
	{
		_logger = logger;
	}

	public void DoSomething()
	{
		_logger.LogInformation("SampleServiceInWorker is doing something...");
		Console.WriteLine("SampleServiceInWorker is doing something...");
	}
}
