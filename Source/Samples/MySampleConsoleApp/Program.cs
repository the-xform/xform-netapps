// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySampleConsoleApp;
using XForm.NetApps.Builders.Console;
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

// See launchSettings.json for adding additional external configuration files 
// and getting them loaded into IConfiguration.

// Sample of logging before the injection has been configured. Will log on debug console.
var logger = logger_factory.CreateLogger("MySampleConsoleAppHost");
logger.LogInformation("***** Starting Console App Host *****");

var console_app_builder = ConsoleAppBuilder.CreateHostBuilder(new ConsoleAppOptions
{
	AppName = "MySampleConsole",
	Args = args
});

// Application services are injected in SampleExternalServiceInjector which invoked from within CommonAppBuilder.
// The service injector is configured in the app settings file.
 var host = console_app_builder.Build();

var sample_service = host.Services.GetRequiredService<ISampleService>();
sample_service.Run();

//host.Run();