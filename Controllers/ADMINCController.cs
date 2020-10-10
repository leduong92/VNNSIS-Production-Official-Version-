using Microsoft.AspNetCore.Mvc;
using VNNSIS.Controllers;

namespace MvcIdentity.Controllers
{
    public class ADMINCController : BaseController
    {
		// [Authorize(Roles="Admin")]
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Index1()
        {
            return View();
        }
        public IActionResult Index2()
        {
            return View();
        }
    }
}