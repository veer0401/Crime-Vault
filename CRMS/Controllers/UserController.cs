using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace CRMS.Controllers
{
    public class UserController : Controller
    {
        public IActionResult List()
        {
            return View();
        }
        public IActionResult Add()
        {
            return View();
        }
    }
}
