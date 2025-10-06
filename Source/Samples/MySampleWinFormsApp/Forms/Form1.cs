using XForm.NetApps.Providers;
using XForm.Utilities;

namespace MySampleWinFormsApp;

internal partial class Form1 : Form
{
	public Form1(ISampleService service, ISequentialGuidProvider guidProvider, IJsonUtilities jsonUtilities)
	{
		InitializeComponent();

		label1.Text = $"{guidProvider.NewGuid()}";

		service.Run();
	}
}
