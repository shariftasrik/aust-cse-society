using AustCseApp.Data;
using AustCseApp.Data.Helpers;
using AustCseApp.Data.Helpers.Constants;
using AustCseApp.Data.Helpers.Enums;
using AustCseApp.Data.Models;
using AustCseApp.Data.Services;
using AustCseApp.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using AustCseApp.Controllers.Base;


namespace AustCseApp.Controllers
{
    [Authorize]
    public class HomeController: BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostsService _postsService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFilesService _filesService;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger,IPostsService postsService, IHashtagsService hashtagsService, IFilesService filesService, UserManager<User> userManager)
        {
            _logger = logger;
            _postsService = postsService;
            _hashtagsService = hashtagsService;
            _filesService = filesService;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var me = await _userManager.FindByIdAsync(loggedInUserId.Value.ToString());
            if (me == null) return RedirectToLogin();

            var posts = string.IsNullOrWhiteSpace(me.Batch)
                ? new List<Post>()
                : await _postsService.GetBatchPostsAsync(me.Batch, loggedInUserId.Value);

            var isAdmin = await _userManager.IsInRoleAsync(me, AppRoles.Admin);
            return View(new BatchPageVM
            {
                Me = me,
                Posts = posts,
                CanPin = me.IsBatchModerator || isAdmin
            });
        }

        public async Task<IActionResult> Details(int postId)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var post = await _postsService.GetPostByIdAsync(postId, loggedInUserId.Value);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM post)
        {
            //Get the logged in user
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var me = await _userManager.FindByIdAsync(loggedInUserId.Value.ToString());
            if (me == null) return RedirectToLogin();
            if (me.VerificationStatus != VerificationStatus.Verified)
            {
                TempData["BatchError"] = "Your account is waiting for verification, so you cannot post yet.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(me.Batch))
            {
                TempData["BatchError"] = "Add your batch before posting.";
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(post.Content))
            {
                TempData["BatchError"] = "Write something before you post.";
                return RedirectToAction("Index");
            }

            var imageUploadPath = await _filesService.UploadImageAsync(post.Image, ImageFileType.PostImage);

            var newPost = new Post
            {
                Content = post.Content,
                Batch = me.Batch.Trim(),
                Tag = string.IsNullOrWhiteSpace(post.Tag) ? post.PostKind.ToString() : post.Tag.Trim(),
                PostKind = post.PostKind,
                ImageUrl = imageUploadPath,             
                NrOfReports = 0,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                UserId = loggedInUserId.Value
            };          

            await _postsService.CreatePostAsync(newPost);
            await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content);

            //Redirect to the index page
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostLike(PostLikeVm postLikeVM, string returnUrl)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();
            await _postsService.TogglePostLikeAsync(postLikeVM.PostId, loggedInUserId.Value);
            return RedirectAfterPostAction(postLikeVM.PostId, returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM, string returnUrl)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();

            var newComment = new Comment()
            {
                UserId = loggedInUserId.Value,
                PostId = postCommentVM.PostId,
                Content = postCommentVM.Content,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            await _postsService.AddPostCommentAsync(newComment);

            return RedirectAfterPostAction(postCommentVM.PostId, returnUrl);
        }


        [HttpPost]
        public async Task<IActionResult> RemovePostComment(RemoveCommentVM removeCommentVM, string returnUrl)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();
            await _postsService.RemovePostCommentAsync(removeCommentVM.CommentId, loggedInUserId.Value);
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> PinPost(int postId)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();
            var me = await _userManager.FindByIdAsync(loggedInUserId.Value.ToString());
            if (me == null || string.IsNullOrWhiteSpace(me.Batch)) return RedirectToAction("Index");
            var isAdmin = await _userManager.IsInRoleAsync(me, AppRoles.Admin);
            if (!me.IsBatchModerator && !isAdmin) return Forbid();
            await _postsService.PinNoticeAsync(postId, loggedInUserId.Value, me.Batch);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PostRemove(PostRemoveVM postRemoveVM)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return RedirectToLogin();
            var postRemoved = await _postsService.RemovePostAsync(postRemoveVM.PostId, loggedInUserId.Value);
            if (postRemoved != null)
                await _hashtagsService.ProcessHashtagsForRemovedPostAsync(postRemoved.Content);
            return RedirectToAction("Index");
        }
        private IActionResult RedirectAfterPostAction(int postId, string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Details", new { postId });
        }
    }

}