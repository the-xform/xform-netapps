
using XForm.NetApps.Builders.WinForms;

var webapi_builder = WebApiBuilder.CreateHostBuilder(new WebApiOptions
{
	ApiName = "MySampleWebApi",
	Args = args
});

webapi_builder.ConfigureWebHostDefaults(webBuilder =>
{
	webBuilder
	.ConfigureServices(services =>
	{
		services.AddControllers();
	})
	.Configure((webHostBuilderContext, applicationBuilder) =>
	{
		if (webHostBuilderContext.HostingEnvironment.IsDevelopment())
		{
			applicationBuilder.UseDeveloperExceptionPage();
		}

		applicationBuilder.UseHttpsRedirection();
		applicationBuilder.UseStaticFiles();
		applicationBuilder.UseRouting();

		applicationBuilder.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});
	});
});

webapi_builder.Build().Run();