using AustCseApp.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace AustCseApp.Data.Services
{
    public class PostsService : IPostsService
    {
        private readonly AppDbContext _context;
        public PostsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetAllPostsAsync(int loggedInUserId)
        {
            var allPosts = await _context.Posts
                .Where(n => (!n.IsPrivate || n.UserId == loggedInUserId) && !n.IsDeleted)
                .Include(n => n.User)
                .Include(n => n.Likes)
                .Include(n => n.Favorites)
                .Include(n => n.Comments).ThenInclude(n => n.User)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();

            return allPosts;
        }

        public async Task<List<Post>> GetBatchPostsAsync(string batch, int loggedInUserId)
        {
            return await _context.Posts
                .Where(n => n.Batch == batch && !n.IsDeleted && (!n.IsPrivate || n.UserId == loggedInUserId))
                .Include(n => n.User)
                .Include(n => n.Likes)
                .Include(n => n.Favorites)
                .Include(n => n.Comments).ThenInclude(n => n.User)
                .OrderByDescending(n => n.IsPinned)
                .ThenByDescending(n => n.DateCreated)
                .ToListAsync();
        }

        public async Task PinNoticeAsync(int postId, int userId, string batch)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(n =>
                n.Id == postId && n.Batch == batch && !n.IsDeleted && n.PostKind == PostKind.Notice);
            if (post == null) return;

            var pinned = await _context.Posts
                .Where(n => n.Batch == batch && n.IsPinned && n.Id != post.Id)
                .ToListAsync();
            foreach (var item in pinned)
                item.IsPinned = false;

            post.IsPinned = !post.IsPinned;
            post.DateUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<List<Post>> GetPostsByUserAsync(int profileUserId, int loggedInUserId)
        {
            return await _context.Posts
                .Where(n => n.UserId == profileUserId
                    && !n.IsDeleted
                    && (!n.IsPrivate || n.UserId == loggedInUserId))
                .Include(n => n.User)
                .Include(n => n.Likes)
                .Include(n => n.Favorites)
                .Include(n => n.Comments).ThenInclude(n => n.User)
                .OrderByDescending(n => n.DateCreated)
                .ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(int postId, int loggedInUserId)
        {
            var postDb = await _context.Posts
               .Include(n => n.User)
               .Include(n => n.Likes)
               .Include(n => n.Favorites)
               .Include(n => n.Comments).ThenInclude(n => n.User)
               .FirstOrDefaultAsync(n => n.Id == postId && !n.IsDeleted);

            if (postDb == null) return null;
            if (postDb.IsPrivate && postDb.UserId != loggedInUserId) return null;
            return postDb;
        }

        public async Task<List<Post>> GetAllFavoritedPostsAsync(int loggedInUserId)
        {
            var allFavoritedPosts = await _context.Favorites
                .Include(f => f.Post.User)
                .Include(f => f.Post.Comments)
                    .ThenInclude(c => c.User)
                .Include(f => f.Post.Likes)
                .Include(f => f.Post.Favorites)
                .Where(n => n.UserId == loggedInUserId
                    && !n.Post.IsDeleted
                    && (!n.Post.IsPrivate || n.Post.UserId == loggedInUserId))
                .OrderByDescending(f => f.DateCreated)
                .Select(n => n.Post)
                .ToListAsync();

            return allFavoritedPosts;
        }

        public async Task AddPostCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<Post> CreatePostAsync(Post post)
        {           
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            return post;
        }

        public async Task<Post> RemovePostAsync(int postId, int userId)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(n => n.Id == postId && n.UserId == userId && !n.IsDeleted);

            if (post == null) return null;

            post.IsDeleted = true;
            post.DateUpdated = DateTime.UtcNow;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return post;
        }



        public async Task RemovePostCommentAsync(int commentId, int userId)
        {
            var commentDb = await _context.Comments.FirstOrDefaultAsync(n => n.Id == commentId && n.UserId == userId);
            if (commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemovePostCommentAsync(int commentId)
        {
            var commentDb = _context.Comments.FirstOrDefault(n => n.Id == commentId);

            if (commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }
        }

        public Task ReportPostAsync(int postId, int userId)
        {
            throw new NotImplementedException();
        }

        public async Task TogglePostFavoriteAsync(int postId, int userId)
        {
            //check if user has already favorited the post
            var favorite = await _context.Favorites
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite()
                {
                    PostId = postId,
                    UserId = userId
                };
                await _context.Favorites.AddAsync(newFavorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TogglePostLikeAsync(int postId, int userId)
        {
            //check if user has already liked the post
            var like = await _context.Likes
                .Where(l => l.PostId == postId && l.UserId == userId)
                .FirstOrDefaultAsync();

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postId,
                    UserId = userId
                };
                await _context.Likes.AddAsync(newLike);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TogglePostVisibilityAsync(int postId, int userId)
        {
            //get post by id and loggedin user id
            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postId && l.UserId == userId);

            if (post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }
        }
    }
}
