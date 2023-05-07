using Microsoft.AspNetCore.Mvc;

namespace Foundry.Controllers
{
    [ApiController]
	[Route("api/[controller]")]
	public class HealthController : ControllerBase
	{
		public HealthController()
		{
		}

		[HttpGet("cop")]
		public ActionResult COP()
		{
			return Redirect("/cop/index.html");
		}

		[HttpGet("platform")]
		public ActionResult Platform()
		{
			return Redirect("/platform/index.html");
		}





	}
}
