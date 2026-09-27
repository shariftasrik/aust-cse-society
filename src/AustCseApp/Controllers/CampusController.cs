using AustCseApp.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AustCseApp.Controllers
{
    [Authorize]
    public class CampusController : BaseController
    {
        public IActionResult More() => View();
        public IActionResult StudyMaterials() => View();
        public IActionResult VideoArchive() => View();
        public IActionResult Quiz() => View();
    }
}