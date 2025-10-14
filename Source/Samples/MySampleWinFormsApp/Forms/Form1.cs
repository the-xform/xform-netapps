using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XForm.NetApps.Interfaces;
using XForm.Utilities;

namespace MySampleWinFormsApp;

internal partial class Form1 : Form
{
	public Form1(
		IConfiguration config,
		ISampleService service,
		ISequentialGuidProvider guidProvider,
		IJsonUtilities jsonUtilities,
		[FromKeyedServices("Db1ConnectionString")] IDbContextProvider dbContext1,
		[FromKeyedServices("Db2ConnectionString")] IDbContextProvider dbContext2)
	{
		InitializeComponent();

		label1.Text = $"{guidProvider.NewGuid()}";

		textBox1.Text += ($"MySampleSetting					: {config["MySampleSetting"]} {Environment.NewLine}");
		textBox1.Text += ($"DbConnectionString				: {config["ConnectionStrings:DbConnectionString"]} {Environment.NewLine}");
		textBox1.Text += ($"AdditionalConfigKey				: {config["AdditionalConfigKey"]} {Environment.NewLine}");
		textBox1.Text += ($"DbContext1 ConnectionString		: {dbContext1.Connection?.ConnectionString} {Environment.NewLine}");
		textBox1.Text += ($"DbContext2 ConnectionString		: {dbContext2.Connection?.ConnectionString} {Environment.NewLine}");

		service.Run();
	}
}
