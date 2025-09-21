using AustCseApp.Data;
using AustCseApp.Data.Helpers;
using AustCseApp.Data.Helpers.Enums;
using AustCseApp.Data.Models;
using AustCseApp.Data.Services;
using AustCseApp.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;


namespace AustCseApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostsService _postsService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFilesService _filesService;

        public HomeController(ILogger<HomeController> logger,IPostsService postsService, IHashtagsService hashtagsService, IFilesService filesService)
        {
            _logger = logger;
            _postsService = postsService;
            _hashtagsService = hashtagsService;
            _filesService = filesService;
        }


        public async Task<IActionResult> Index()
        {
            int loggedInUserId = 1;

            var allPosts = await _postsService.GetAllPostsAsync(loggedInUserId);

            return View(allPosts);
        }

        public async Task<IActionResult> Details(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM post)
        {
            //Get the logged in user
            int loggedInUser = 1;

            var imageUploadPath = await _filesService.UploadImageAsync(post.Image, ImageFileType.PostImage);

            //Create a new post
            var newPost = new Post
            {
                Content = post.Content,
                Batch = string.IsNullOrWhiteSpace(post.Batch) ? null : post.Batch.Trim(),
                Tag = string.IsNullOrWhiteSpace(post.Tag) ? null : post.Tag.Trim(),
                ImageUrl = imageUploadPath,             
                NrOfReports = 0,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
                UserId = loggedInUser
            };          

            await _postsService.CreatePostAsync(newPost);
            await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content);

            //Redirect to the index page
            return RedirectToAction("Index");
        }
    
        [HttpPost]
        public async Task<IActionResult> TogglePostLike(PostLikeVm postLikeVM)
        {
            int loggedInUserId = 1;

            await _postsService.TogglePostLikeAsync(postLikeVM.PostId, loggedInUserId);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TogglePostVisibility(PostVisibilityVM postVisibilityVM)
        {
            int loggedInUserId = 1;

            await _postsService.TogglePostVisibilityAsync(postVisibilityVM.PostId, loggedInUserId);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddPostComment(PostCommentVM postCommentVM)
        {
            int loggedInUserId = 1;
            
            //Creat a post object
            var newComment = new Comment()
            {
                UserId = loggedInUserId,
                PostId = postCommentVM.PostId,
                Content = postCommentVM.Content,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            };

            await _postsService.AddPostCommentAsync(newComment);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> RemovePostComment(RemoveCommentVM removeCommentVM)
        {
            await _postsService.RemovePostCommentAsync(removeCommentVM.CommentId);

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> TogglePostFavorite(PostFavoriteVM postFavoriteVM)
        {
            int loggedInUserId = 1;

            await _postsService.TogglePostFavoriteAsync(postFavoriteVM.PostId, loggedInUserId);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> PostRemove(PostRemoveVM postRemoveVM)
        {
            var postRemoved = await _postsService.RemovePostAsync(postRemoveVM.PostId);
            await _hashtagsService.ProcessHashtagsForRemovedPostAsync(postRemoved.Content);


            return RedirectToAction("Index");
        }

    }
}
