using AustCseApp.Controllers.Base;
using AustCseApp.Data.Models;
using AustCseApp.Data.Services;
using AustCseApp.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AustCseApp.Controllers
{
    [Authorize]
    public class SettingsController : BaseController
    {
        private readonly IUsersService _usersService;
        private readonly IFilesService _filesService;
        private readonly UserManager<User> _userManager;
        public SettingsController(IUsersService usersService, IFilesService filesService, UserManager<User> userManager)
        {
            _usersService = usersService;
            _filesService = filesService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            //var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var userDb = await _usersService.GetUser(int.Parse(loggedInUserId));

            var loggedInUser = await _userManager.GetUserAsync(User);

            return View(loggedInUser);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(UpdateProfilePictureVM profilePictureVM)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var uploadedProfilePictureUrl = await _filesService.UploadImageAsync(profilePictureVM.ProfilePictureImage, Data.Helpers.Enums.ImageFileType.ProfilePicture);

            await _usersService.UpdateUserProfilePicture(loggedInUserId.Value, uploadedProfilePictureUrl);

            return RedirectToAction("Index");
        }

        //[HttpPost]
        //public async Task<IActionResult> UpdateProfile(UpdateProfileVM profileVM)
        //{
        //    return RedirectToAction("Index");
        //}


    }
}
