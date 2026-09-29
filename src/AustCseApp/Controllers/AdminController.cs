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
                return RedirectToAction("Index");
            }

            var isFundamental = track == "Fundamental";
            if (!isFundamental && semester is null or < 1 or > 8)
            {
                TempData["AdminError"] = "Pick a semester from 1 to 8.";
                return RedirectToAction("Index");
            }

            await _learn.AddCourseAsync(new Course
            {
                Code = code.Trim(),
                Title = title.Trim(),
                Track = isFundamental ? CourseTrack.Fundamental : CourseTrack.Semester,
                SemesterNumber = isFundamental ? null : semester,
                SyllabusEra = isFundamental ? "Career" : "Department"
            });
            return RedirectToAction("Index");
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
