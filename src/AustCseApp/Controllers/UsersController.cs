using AustCseApp.Controllers.Base;
using AustCseApp.Data.Services;
using AustCseApp.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AustCseApp.Controllers
{
    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUsersService _usersService;
        private readonly IPostsService _postsService;

        public UsersController(IUsersService usersService, IPostsService postsService)
        {
            _usersService = usersService;
            _postsService = postsService;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var users = await _usersService.GetOtherUsersAsync(loggedInUserId.Value, 50);
            return View(users);
        }

        public async Task<IActionResult> Details(int userId)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var profileUser = await _usersService.GetUser(userId);
            if (profileUser == null) return NotFound();

            var posts = await _postsService.GetPostsByUserAsync(userId, loggedInUserId.Value);

            return View(new UserProfileVM
            {
                ProfileUser = profileUser,
                Posts = posts
            });
        }
    }
}