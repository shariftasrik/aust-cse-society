using AustCseApp.Data.Helpers.Constants;
using AustCseApp.Data.Models;
using AustCseApp.ViewModels.Authentication;
using AustCseApp.ViewModels.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AustCseApp.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AuthenticationController(UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var existingUser = await _userManager.FindByEmailAsync(loginVM.Email);
            if (existingUser == null)
            {
                ModelState.AddModelError("", "Invalid email or password. Please, try again");
                return View(loginVM);
            }

            var existingUserClaims = await _userManager.GetClaimsAsync(existingUser);
            if (!existingUserClaims.Any(c => c.Type == CustomClaim.FullName))
                await _userManager.AddClaimAsync(existingUser, new Claim(CustomClaim.FullName, existingUser.FullName));

            var result = await _signInManager.PasswordSignInAsync(
                existingUser.UserName, loginVM.Password, loginVM.RememberMe, false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Invalid login attempt");
            return View(loginVM);
        }

        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            if (registerVM.AccountKind == AccountKind.CurrentStudent)
            {
                if (string.IsNullOrWhiteSpace(registerVM.StudentId))
                    ModelState.AddModelError(nameof(registerVM.StudentId), "Student ID is required");
                if (registerVM.CurrentSemester is null or < 1 or > 8)
                    ModelState.AddModelError(nameof(registerVM.CurrentSemester), "Semester must be from 1 to 8");
            }
            else if (registerVM.GraduationYear is null or < 1995 or > 2100)
            {
                ModelState.AddModelError(nameof(registerVM.GraduationYear), "Enter your graduation year");
            }

            if (!ModelState.IsValid)
                return View(registerVM);

            var newUser = new User()
            {
                FullName = $"{registerVM.FirstName} {registerVM.LastName}",
                Email = registerVM.Email,
                UserName = registerVM.Email,
                AccountKind = registerVM.AccountKind,
                Batch = registerVM.Batch.Trim(),
                StudentId = registerVM.StudentId?.Trim(),
                CurrentSemester = registerVM.CurrentSemester,
                GraduationYear = registerVM.GraduationYear,
                Company = registerVM.Company?.Trim(),
                VerificationStatus = VerificationStatus.Pending
            };

            var existingUser = await _userManager.FindByEmailAsync(registerVM.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(registerVM);
            }

            var result = await _userManager.CreateAsync(newUser, registerVM.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, AppRoles.User);
                await _userManager.AddClaimAsync(newUser, new Claim(CustomClaim.FullName, newUser.FullName));
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(registerVM);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordVM updatePasswordVM)
        {
            if (updatePasswordVM.NewPassword != updatePasswordVM.ConfirmPassword)
            {
                TempData["PasswordError"] = "Passwords do not match";
                TempData["ActiveTab"] = "Password";

                return RedirectToAction("Index", "Settings");
            }

            var loggedInUser = await _userManager.GetUserAsync(User);
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(loggedInUser, updatePasswordVM.CurrentPassword);

            if (!isCurrentPasswordValid)
            {
                TempData["PasswordError"] = "Current password is invalid";
                TempData["ActiveTab"] = "Password";
                return RedirectToAction("Index", "Settings");
            }

            var result = await _userManager.ChangePasswordAsync(loggedInUser, updatePasswordVM.CurrentPassword, updatePasswordVM.NewPassword);

            if (result.Succeeded)
            {
                TempData["PasswordSuccess"] = "Password updated successfully";
                TempData["ActiveTab"] = "Password";
                await _signInManager.RefreshSignInAsync(loggedInUser);
            }

            return RedirectToAction("Index", "Settings");
        }


        [HttpPost]
        public async Task<IActionResult> UpdateProfile(UpdateProfileVM profileVM)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
                return RedirectToAction("Login");

            loggedInUser.FullName = profileVM.FullName;
            loggedInUser.UserName = profileVM.UserName;
            loggedInUser.Bio = profileVM.Bio;
            loggedInUser.CurrentSemester = profileVM.CurrentSemester;
            loggedInUser.Company = profileVM.Company;
            loggedInUser.JobTitle = profileVM.JobTitle;
            loggedInUser.CanRefer = profileVM.CanRefer && loggedInUser.AccountKind == AccountKind.Alumni;
            if (string.IsNullOrWhiteSpace(loggedInUser.Batch) && !string.IsNullOrWhiteSpace(profileVM.Batch))
                loggedInUser.Batch = profileVM.Batch.Trim();

            var result = await _userManager.UpdateAsync(loggedInUser);
            if (!result.Succeeded)
            {
                TempData["UserProfileError"] = "User profile could not be updated";
                TempData["ActiveTab"] = "Profile";
            }

            await _signInManager.RefreshSignInAsync(loggedInUser);
            return RedirectToAction("Index", "Settings");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
