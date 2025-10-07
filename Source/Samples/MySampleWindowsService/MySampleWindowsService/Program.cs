using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XForm.Common.Apps.Builders.Application;
using XForm.NetApps.Builders.WinService;
using XForm.NetApps.Hosts.WinService;
using XForm.Utilities;

#region - Global exception handling -

var logger_factory = LoggerFactory.Create(config =>
{
	config.AddConsole();
	config.AddDebug();
	config.AddEventSourceLogger();
});

AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
{
	var details = new
	{
		ErrorCode = "UnhandledException",
		Timestamp = DateTimeOffset.Now.ToString("O"),
		Exception = eventArgs.ExceptionObject as Exception
	};
	var logger = logger_factory.CreateLogger("UnhandledExceptionLogger");
	logger.LogCritical(JsonUtilities.ConvertToJson(details));
};

#endregion - Global exception handling -

// Sample of logging before the injection has been configured. Will log on debug console.
var logger = logger_factory.CreateLogger("SampleWindowsServiceHost");
logger.LogInformation("***** Starting Sample Windows Service Host *****");

ApplicationBuilder.ConfigureThreadsForApplication(25, 25);

var windows_service_builder = WindowsServiceBuilder.CreateAppHostBuilder(new WindowsServiceOptions
{
	ServiceName = "MySampleWindowsService",
	Args = args,
	ApplicationSetup = (configBuilder, services) =>
	{
		// Everything is registered in common, nothing to register here.
		// Defining this delegate just satifies the service options requirements.
	}
});

// Test that the additional configuration value from the configuration is available.
logger.LogInformation($"Additional Config Value: {windows_service_builder.Configuration.GetValue<string>("AdditionalConfigKey")}");

var app = windows_service_builder.Build();
app.Run();

logger.LogInformation("***** Exiting Windows Service Host *****");
