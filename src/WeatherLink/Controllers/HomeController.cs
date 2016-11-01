using Microsoft.AspNetCore.Mvc;

namespace WeatherLink.Controllers
{
	/// <summary>
	///
	/// </summary>
	[ApiExplorerSettings(IgnoreApi = true)]
	[Route("/")]
	public class HomeController : Controller
	{
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public IActionResult Index()
		{
            return Redirect("~/swagger/ui");
        }
	}
}