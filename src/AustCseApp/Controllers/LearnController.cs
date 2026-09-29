using AustCseApp.Controllers.Base;
using AustCseApp.Data.Helpers.Constants;
using AustCseApp.Data.Models;
using AustCseApp.Data.Services;
using AustCseApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace AustCseApp.Controllers
{
    [Authorize]
    public class LearnController : BaseController
    {
        private readonly ILearnService _learn;
        private readonly UserManager<User> _users;
        private readonly PrivateFileStore _files;

        public LearnController(ILearnService learn, UserManager<User> users, PrivateFileStore files)
        {
            _learn = learn;
            _users = users;
            _files = files;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> Semester(int id)
        {
            if (id is < 1 or > 8) return NotFound();
            ViewBag.Semester = id;
            return View(await _learn.GetSemesterCoursesAsync(id));
        }

        public async Task<IActionResult> Fundamentals()
        {
            return View(await _learn.GetFundamentalCoursesAsync());
        }

        public async Task<IActionResult> Course(int id)
        {
            var course = await _learn.GetCourseAsync(id);
            if (course == null) return NotFound();
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            var me = await _users.FindByIdAsync(userId.Value.ToString());
            var isAdmin = me != null && await _users.IsInRoleAsync(me, AppRoles.Admin);
            ViewBag.Me = me;
            ViewBag.Resources = await _learn.GetVisibleResourcesAsync(id, userId.Value, isAdmin);
            return View(course);
        }

        public async Task<IActionResult> Saved()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            return View(await _learn.GetSavedAsync(userId.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int courseId, ResourceKind kind, string title, string? youtubeUrl, string? sessionLabel, int? year, bool isOriginalOrOpenLicense, IFormFile? file)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            var me = await _users.FindByIdAsync(userId.Value.ToString());
            if (me == null || me.VerificationStatus != VerificationStatus.Verified)
            {
                TempData["LearnError"] = "Only a verified member can add a resource.";
                return RedirectToAction("Course", new { id = courseId });
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                TempData["LearnError"] = "Add a title.";
                return RedirectToAction("Course", new { id = courseId });
            }

            string? storedFile = null;
            string? video = null;
            if (kind == ResourceKind.Video)
            {
                video = YoutubeEmbed.Normalize(youtubeUrl);
                if (video == null)
                {
                    TempData["LearnError"] = "Paste a YouTube link.";
                    return RedirectToAction("Course", new { id = courseId });
                }
            }
            else
            {
                if (kind is ResourceKind.Note or ResourceKind.Book && !isOriginalOrOpenLicense)
                {
                    TempData["LearnError"] = "Notes and books must be your own writing or openly licensed. Commercial textbook scans are not accepted.";
                    return RedirectToAction("Course", new { id = courseId });
                }

                storedFile = await _files.SavePdfAsync(file, "library");
                if (storedFile == null)
                {
                    TempData["LearnError"] = "Upload a PDF under 15 MB.";
                    return RedirectToAction("Course", new { id = courseId });
                }
            }

            await _learn.SubmitAsync(new LearningResource
            {
                CourseId = courseId,
                Kind = kind,
                Title = title.Trim(),
                YoutubeUrl = video,
                FilePath = storedFile,
                SessionLabel = sessionLabel?.Trim(),
                Year = year,
                IsOriginalOrOpenLicense = isOriginalOrOpenLicense || kind == ResourceKind.Video,
                SubmittedByUserId = userId.Value
            });

            TempData["LearnMessage"] = "Submitted. It appears on the course page after a reviewer approves it.";
            return RedirectToAction("Course", new { id = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> Save(int resourceId, int courseId)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            await _learn.ToggleSaveAsync(resourceId, userId.Value);
            return RedirectToAction("Course", new { id = courseId });
        }

        public async Task<IActionResult> File(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            var resource = await _learn.GetResourceAsync(id);
            if (resource == null || string.IsNullOrWhiteSpace(resource.FilePath)) return NotFound();

            var me = await _users.FindByIdAsync(userId.Value.ToString());
            var isAdmin = me != null && await _users.IsInRoleAsync(me, AppRoles.Admin);
            var allowed = resource.ApprovalStatus == ApprovalStatus.Approved
                || resource.SubmittedByUserId == userId
                || isAdmin;
            if (!allowed) return Forbid();

            var path = _files.AbsolutePath(resource.FilePath);
            if (path == null) return NotFound();
            return PhysicalFile(path, "application/pdf");
        }
    }

    public static class YoutubeEmbed
    {
        public static string? Normalize(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)) return null;
            var host = uri.Host.Replace("www.", "", StringComparison.OrdinalIgnoreCase);
            string? id = null;
            if (host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase))
                id = uri.AbsolutePath.Trim('/');
            else if (host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
            {
                var query = QueryHelpers.ParseQuery(uri.Query);
                if (query.TryGetValue("v", out var v)) id = v.ToString();
                else
                {
                    var parts = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && (parts[0] == "embed" || parts[0] == "shorts"))
                        id = parts[1];
                }
            }

            if (string.IsNullOrWhiteSpace(id) || id.Length > 20) return null;
            return "https://www.youtube.com/embed/" + id;
        }
    }
}
