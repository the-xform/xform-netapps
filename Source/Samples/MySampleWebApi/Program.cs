
using Microsoft.OpenApi.Models;
using XForm.NetApps.Builders.WebApi;

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
app.Run();