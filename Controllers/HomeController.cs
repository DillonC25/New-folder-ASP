using Microsoft.AspNetCore.Mvc;

namespace SeacoastUniversity.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Directory() => View();
        public IActionResult WeekAtAGlance() => View();
    }
}
