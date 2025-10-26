using Microsoft.AspNetCore.Mvc;
using XForm.NetApps.Attibutes.Web;
using XForm.NetApps.Interfaces;

namespace MySampleWebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HomeController : ControllerBase
	{
		private readonly ISampleService _sampleServices;
		private readonly IDbContextProvider _dbContext1;
		private readonly IDbContextProvider _dbContext2;

		public HomeController(ISampleService sampleServices,
			[FromKeyedServices("XformConnectionString")] IDbContextProvider dbContext1,
			[FromKeyedServices("SomeConnectionString")] IDbContextProvider dbContext2)
		{
			_dbContext1 = dbContext1;
			_dbContext2 = dbContext2;

			_sampleServices = sampleServices;
			_sampleServices.Run();
		}

		[HttpGet]
		[Route("Index")]
		[DbContext(dbConnectionStringName: "SomeConnectionString")]
		public object Index()
		{
			return new
			{
				Message = "Welcome to MySampleWebApi! The API is up and running.",
				DbContext1ConnectionString = _dbContext1.Connection?.ConnectionString,
				DbContext2ConnectionString = _dbContext2.Connection?.ConnectionString
			};
		}

		[HttpGet]
		[Route("Foo")]
		public IActionResult Foo()
		{
			return new ContentResult()
			{
				Content = "Foo",
				ContentType = "text/plain",
				StatusCode = 200
			};
		}
	}
}
