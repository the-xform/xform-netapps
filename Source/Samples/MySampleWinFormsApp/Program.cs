using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XForm.NetApps.Builders.WinForms;
using XForm.NetApps.Interfaces;
using XForm.Utilities;
using XForm.Utilities.Validations;

namespace MySampleWinFormsApp;

internal static class Program
{
	/// <summary>
	///  The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main(string[] args)
	{
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
		var logger = logger_factory.CreateLogger("MySampleWinFormsApp");
		logger.LogInformation("***** Starting Windows App Host *****");

		// Example 1: Using HostBuilder
		//var winforms_app_builder = WinFormsAppBuilder.CreateHostBuilder(new WinFormsAppOptions
		//{
		//	AppName = "MySampleWinformsApp",
		//	Args = args
		//});

		//Example 2: Using HostApplicationBuilder
		var winforms_app_builder = WinFormsAppBuilder.CreateAppHostBuilder(new WinFormsAppOptions
		{
			AppName = "MySampleWinformsApp",
			Args = args
		});

		var host = winforms_app_builder.Build();

		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();

		// Check if the default injections were added
		var guid_provider = host.Services.GetRequiredService<ISequentialGuidProvider>();
		var json_utilities = host.Services.GetRequiredService<IJsonUtilities>();
		var certificate_provider = host.Services.GetRequiredService<ICertificateProvider>();
		var config_proxy_provider = host.Services.GetRequiredService<IConfigProxyProvider>();

		Xssert.IsNotNull(guid_provider);
		Xssert.IsNotNull(json_utilities);
		Xssert.IsNotNull(certificate_provider);
		Xssert.IsNotNull(config_proxy_provider);

		// Load the form via DI. The DI has been configured in SampleExternalServiceInjector which is invoked from within CommonAppBuilder.
		var main_form = host.Services.GetRequiredService<Form1>();
		Application.Run(main_form);
	}
}