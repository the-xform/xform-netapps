namespace XForm.NetApps.ConfigurationSettings;

public class SqlConnectionSettings
{
	public bool IsEnabled { get; set; }
	public Dictionary<string, string>? ConnectionStrings { get; set; }
}
