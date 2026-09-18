using Microsoft.AspNetCore.Mvc;

namespace FISIOSPORT.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}