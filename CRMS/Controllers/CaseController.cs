using Microsoft.AspNetCore.Mvc;

namespace CRMS.Controllers
{
    public class CaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
