using Microsoft.AspNetCore.Mvc;

namespace GreenGardenClient.Controllers.Error
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
