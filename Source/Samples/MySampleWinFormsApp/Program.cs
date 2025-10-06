using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XForm.NetApps.Builders.WinForms;
using XForm.Utilities;

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

		var winforms_app_builder = WinFormsAppBuilder.CreateHostBuilder(new WinFormsAppOptions
		{
			AppName = "MySampleWinformsApp",
			Args = args
		});

		var host = winforms_app_builder.Build();

		// To customize application configuration such as set high DPI settings or default font,
		// see https://aka.ms/applicationconfiguration.
		ApplicationConfiguration.Initialize();

		// Load the form via DI. The DI has been configured in SampleExternalServiceInjector which is invoked from within CommonAppBuilder.
		var main_form = host.Services.GetRequiredService<Form1>();
		Application.Run(main_form);
	}
}