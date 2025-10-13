// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySampleConsoleApp;
using XForm.NetApps.Builders.Console;
using XForm.NetApps.Interfaces;
using XForm.Utilities;
using XForm.Utilities.Validations;

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

// Example 1: Using HostBuilder
//var console_app_builder = ConsoleAppBuilder.CreateHostBuilder(new ConsoleAppOptions
//{
//	AppName = "MySampleConsole",
//	Args = args
//});

// Example 2: Using HostApplicationBuilder
var console_app_builder = ConsoleAppBuilder.CreateAppHostBuilder(new ConsoleAppOptions
{
	AppName = "MySampleConsole",
	Args = args
});

// Application services are injected in SampleExternalServiceInjector which invoked from within CommonAppBuilder.
// The service injector is configured in the app settings file.
var host = console_app_builder.Build();

// Check if the default injections were added
var guid_provider = host.Services.GetRequiredService<ISequentialGuidProvider>();
var json_utilities = host.Services.GetRequiredService<IJsonUtilities>();
var certificate_provider = host.Services.GetRequiredService<ICertificateProvider>();
var config_proxy_provider = host.Services.GetRequiredService<IConfigProxyProvider>();

Xssert.IsNotNull(guid_provider);
Xssert.IsNotNull(json_utilities);
Xssert.IsNotNull(certificate_provider);
Xssert.IsNotNull(config_proxy_provider);

// Run sample service
var sample_service = host.Services.GetRequiredService<ISampleService>();
sample_service.Run();

//host.Run();