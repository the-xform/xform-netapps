using Microsoft.AspNetCore.Mvc;

namespace MySampleWebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HomeController : ControllerBase
	{
		private readonly ISampleService _sampleServices;

		public HomeController(ISampleService sampleServices)
		{
			_sampleServices = sampleServices;

			_sampleServices.Run();
		}

		[HttpGet]
		[Route("Index")]
		public object Index()
		{
			return new
			{
				Message = "Welcome to MySampleWebApi! The API is up and running."
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
