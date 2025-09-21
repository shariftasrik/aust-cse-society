using AustCseApp.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace AustCseApp.Controllers
{
    public class UsersController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details(string userId)
        {
            return View();
        }
    }
}
