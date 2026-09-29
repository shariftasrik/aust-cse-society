using AustCseApp.Data.Helpers.Constants;
using AustCseApp.Data.Models;
using AustCseApp.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AustCseApp.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _users;
        private readonly ILearnService _learn;

        public AdminController(UserManager<User> users, ILearnService learn)
        {
            _users = users;
            _learn = learn;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.PendingUsers = await _users.Users
                .Where(u => u.VerificationStatus == VerificationStatus.Pending && !u.IsDeleted)
                .OrderBy(u => u.FullName)
                .ToListAsync();
            ViewBag.PendingResources = await _learn.GetPendingAsync();
            return View();
        }

        public async Task<IActionResult> Content()
        {
            return View(await _learn.GetAllCoursesAsync());
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourse(int id, string code, string title, string track, int? semester)
        {
            var isFundamental = track == "Fundamental";
            if (!isFundamental && semester is null or < 1 or > 8)
            {
                TempData["AdminError"] = "Pick a semester from 1 to 8.";
                return RedirectToAction("Content");
            }

            var saved = await _learn.UpdateCourseAsync(
                id,
                code,
                title,
                semester,
                isFundamental ? CourseTrack.Fundamental : CourseTrack.Semester);
            TempData[saved ? "AdminMessage" : "AdminError"] = saved
                ? "Course updated."
                : "That course could not be updated.";
            return RedirectToAction("Content");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await _learn.DeleteCourseAsync(id);
            TempData["AdminMessage"] = "Course removed.";
            return RedirectToAction("Content");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateResource(int id, string title, string? sessionLabel, int? year)
        {
            var saved = await _learn.UpdateResourceAsync(id, title, sessionLabel, year);
            TempData[saved ? "AdminMessage" : "AdminError"] = saved
                ? "Resource updated."
                : "That resource could not be updated.";
            return RedirectToAction("Content");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteResource(int id)
        {
            await _learn.DeleteResourceAsync(id);
            TempData["AdminMessage"] = "Resource removed.";
            return RedirectToAction("Content");
        }

        [HttpPost]
        public async Task<IActionResult> Verify(int id)
        {
            await SetStatus(id, VerificationStatus.Verified);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            await SetStatus(id, VerificationStatus.Rejected);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Moderator(int id)
        {
            var user = await _users.FindByIdAsync(id.ToString());
            if (user != null)
            {
                user.IsBatchModerator = !user.IsBatchModerator;
                await _users.UpdateAsync(user);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveResource(int id)
        {
            await _learn.SetApprovalAsync(id, ApprovalStatus.Approved);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectResource(int id)
        {
            await _learn.SetApprovalAsync(id, ApprovalStatus.Rejected);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(string code, string title, string track, int? semester)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title))
            {
                TempData["AdminError"] = "A course needs a code and a title.";
                return RedirectToAction("Content");
            }

            var isFundamental = track == "Fundamental";
            if (!isFundamental && semester is null or < 1 or > 8)
            {
                TempData["AdminError"] = "Pick a semester from 1 to 8.";
                return RedirectToAction("Content");
            }

            await _learn.AddCourseAsync(new Course
            {
                Code = code.Trim(),
                Title = title.Trim(),
                Track = isFundamental ? CourseTrack.Fundamental : CourseTrack.Semester,
                SemesterNumber = isFundamental ? null : semester,
                SyllabusEra = isFundamental ? "Career" : "Department"
            });
            TempData["AdminMessage"] = "Course added.";
            return RedirectToAction("Content");
        }

        private async Task SetStatus(int id, VerificationStatus status)
        {
            var user = await _users.FindByIdAsync(id.ToString());
            if (user == null) return;
            user.VerificationStatus = status;
            await _users.UpdateAsync(user);
        }
    }
}
