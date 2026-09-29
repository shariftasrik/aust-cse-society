using AustCseApp.Controllers.Base;
using AustCseApp.Data.Models;
using AustCseApp.Data.Services;
using AustCseApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AustCseApp.Controllers
{
    [Authorize]
    public class CareerController : BaseController
    {
        private readonly ICareerService _career;
        private readonly UserManager<User> _users;
        private readonly PrivateFileStore _files;

        public CareerController(ICareerService career, UserManager<User> users, PrivateFileStore files)
        {
            _career = career;
            _users = users;
            _files = files;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Hiring = await _career.GetOpenHiringAsync();
            var userId = GetUserId();
            User? me = null;
            if (userId != null)
                me = await _users.FindByIdAsync(userId.Value.ToString());
            ViewBag.Me = me;
            ViewBag.Referrals = me?.CanRefer == true && me.AccountKind == AccountKind.Alumni
                ? await _career.GetOpenRequestsAsync()
                : new List<ReferralRequest>();
            ViewBag.Mine = userId == null ? null : await _career.GetOpenRequestAsync(userId.Value);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateReferral(string targetRole, string skills, string note, IFormFile? cv)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            var me = await _users.FindByIdAsync(userId.Value.ToString());
            if (me == null || me.VerificationStatus != VerificationStatus.Verified)
            {
                TempData["CareerError"] = "Only a verified member can ask for a referral.";
                return RedirectToAction("Index");
            }

            var eligible = me.AccountKind == AccountKind.Alumni || me.CurrentSemester is >= 7;
            if (!eligible)
            {
                TempData["CareerError"] = "Referral requests open from 7th semester, and for alumni who are still looking.";
                return RedirectToAction("Index");
            }

            var cvPath = await _files.SavePdfAsync(cv, "cv");
            if (cvPath == null)
            {
                TempData["CareerError"] = "Attach your CV as a PDF.";
                return RedirectToAction("Index");
            }

            var created = await _career.CreateRequestAsync(new ReferralRequest
            {
                UserId = userId.Value,
                TargetRole = targetRole?.Trim() ?? "",
                Skills = skills?.Trim() ?? "",
                Note = note?.Trim() ?? "",
                CvPath = cvPath
            });
            TempData[created ? "CareerMessage" : "CareerError"] = created
                ? "Your request is open. Alumni who opt in can ask for your CV."
                : "You already have an open request. Close it before posting another.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Ask(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            var me = await _users.FindByIdAsync(userId.Value.ToString());
            if (me == null || me.AccountKind != AccountKind.Alumni || !me.CanRefer || me.VerificationStatus != VerificationStatus.Verified)
                return Forbid();
            await _career.AskForCvAsync(id, userId.Value);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Close(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            await _career.CloseRequestAsync(id, userId.Value);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Cv(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            var request = await _career.GetRequestAsync(id);
            if (request == null || string.IsNullOrWhiteSpace(request.CvPath)) return NotFound();
            var allowed = request.UserId == userId || request.Contacts.Any(c => c.AlumniUserId == userId);
            if (!allowed) return Forbid();
            var path = _files.AbsolutePath(request.CvPath);
            if (path == null) return NotFound();
            return PhysicalFile(path, "application/pdf");
        }

        [HttpPost]
        public async Task<IActionResult> CreateHiring(string roleTitle, string jobType, string location, string howToApply, DateTime closingDate)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToLogin();
            var me = await _users.FindByIdAsync(userId.Value.ToString());
            if (me == null || me.AccountKind != AccountKind.Alumni || me.VerificationStatus != VerificationStatus.Verified)
            {
                TempData["CareerError"] = "Only a verified alumnus can post a hiring.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(me.Company))
            {
                TempData["CareerError"] = "Add your company in Settings before posting a job.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(roleTitle) || string.IsNullOrWhiteSpace(howToApply) || closingDate.Date < DateTime.UtcNow.Date)
            {
                TempData["CareerError"] = "Add a role, how to apply, and a closing date that is today or later.";
                return RedirectToAction("Index");
            }

            await _career.CreateHiringAsync(new HiringPost
            {
                UserId = userId.Value,
                RoleTitle = roleTitle.Trim(),
                JobType = string.IsNullOrWhiteSpace(jobType) ? "Full-time" : jobType,
                Location = string.IsNullOrWhiteSpace(location) ? "Dhaka" : location.Trim(),
                HowToApply = howToApply.Trim(),
                ClosingDate = closingDate
            });
            TempData["CareerMessage"] = "Hiring post is live until the closing date.";
            return RedirectToAction("Index");
        }
    }
}
