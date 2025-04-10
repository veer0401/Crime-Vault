using Microsoft.AspNetCore.Mvc;

namespace CRMS.Controllers
{
    public class CaseAPIController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
