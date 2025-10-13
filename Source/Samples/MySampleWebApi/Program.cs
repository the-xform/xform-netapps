
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using XForm.NetApps.Builders.WebApi;
using XForm.NetApps.Interfaces;
using XForm.Utilities;
using XForm.Utilities.Validations;

// Example 1: Using HostBuilder
//var webapi_builder = WebApiBuilder.CreateHostBuilder(new WebApiOptions
//{
//	ApiName = "MySampleWebApi",
//	Args = args
//});

//webapi_builder.ConfigureWebHostDefaults(webBuilder =>
//{
//	webBuilder
//	.ConfigureServices(services =>
//	{
//		services.AddControllers();
//	})
//	.Configure((webHostBuilderContext, applicationBuilder) =>
//	{
//		if (webHostBuilderContext.HostingEnvironment.IsDevelopment())
//		{
//			applicationBuilder.UseDeveloperExceptionPage();
//		}

//		applicationBuilder.UseHttpsRedirection();
//		applicationBuilder.UseStaticFiles();
//		applicationBuilder.UseRouting();

//		applicationBuilder.UseEndpoints(endpoints =>
//		{
//			endpoints.MapControllers();
//		});
//	});
//});
// webapi_builder.Build().Run();



// Example 2: Using WebApplicationBuilder
var webapi_builder = WebApiBuilder.CreateWebApplicationBuilder(new WebApiOptions
{
	ApiName = "MySampleWebApi",
	Args = args
});

webapi_builder.Services.AddControllers();

var app = webapi_builder.Build();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();


// Check if the default injections were added
var guid_provider = app.Services.GetRequiredService<ISequentialGuidProvider>();
var json_utilities = app.Services.GetRequiredService<IJsonUtilities>();
var certificate_provider = app.Services.GetRequiredService<ICertificateProvider>();
var config_proxy_provider = app.Services.GetRequiredService<IConfigProxyProvider>();

Xssert.IsNotNull(guid_provider);
Xssert.IsNotNull(json_utilities);
Xssert.IsNotNull(certificate_provider);
Xssert.IsNotNull(config_proxy_provider);

// Run the webservice
app.Run();