using AustCseApp.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AustCseApp.Data.Services
{
    public interface IPostsService
    {
        Task<List<Post>> GetAllPostsAsync(int loggedInUserId);
        Task<List<Post>> GetBatchPostsAsync(string batch, int loggedInUserId);
        Task PinNoticeAsync(int postId, int userId, string batch);
        Task<List<Post>> GetPostsByUserAsync(int profileUserId, int loggedInUserId);
        Task<Post> GetPostByIdAsync(int postId, int loggedInUserId);
        Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId);
        Task<Post> CreatePostAsync(Post post);
        Task<Post> RemovePostAsync(int postId, int userId);

        Task AddPostCommentAsync(Comment comment);
        Task RemovePostCommentAsync(int commentId, int userId);

        Task TogglePostLikeAsync(int postId, int userId);
        Task TogglePostFavoriteAsync(int postId, int userId);
        Task TogglePostVisibilityAsync(int postId, int userId);
        Task ReportPostAsync(int postId, int userId);
    }
}
