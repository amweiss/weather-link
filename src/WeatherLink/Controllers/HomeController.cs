using Microsoft.AspNetCore.Mvc;

namespace WeatherLink.Controllers
{
	/// <summary>
	/// Provide route for site root.
	/// </summary>
	[ApiExplorerSettings(IgnoreApi = true)]
	[Route("/")]
	public class HomeController : Controller
	{
		/// <summary>
		/// Redirect to swagger at site root.
		/// </summary>
		/// <returns>Redirect action for root URL.</returns>
		[HttpGet]
		public IActionResult Index()
		{
            return Redirect("~/swagger/ui");
        }
	}
}