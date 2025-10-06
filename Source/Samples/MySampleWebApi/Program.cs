
using XForm.NetApps.Builders.WinForms;

var webapi_builder = WebApiBuilder.CreateHostBuilder(new WebApiOptions
{
	ApiName = "MySampleWebApi",
	Args = args
});

var app = webapi_builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();






//var builder = WebApplication.CreateBuilder(args);

//builder.Host.UseContentRoot(AppContext.BaseDirectory);

//// Add services to the container.

////builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
////builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//	app.UseSwagger();
//	app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
