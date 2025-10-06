using Microsoft.AspNetCore.Mvc;

namespace MySampleWebApi.Controllers
{
	public class HomeController : Controller
	{
		public object Index()
		{
			return new
			{
				Message = "Welcome to MySampleWebApi! The API is up and running."
			};
		}
	}
}
