using AustCseApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AustCseApp.Data.Services
{
    public interface ILearnService
    {
        Task<List<Course>> GetSemesterCoursesAsync(int semester);
        Task<List<Course>> GetFundamentalCoursesAsync();
        Task<Course?> GetCourseAsync(int courseId);
        Task<List<LearningResource>> GetVisibleResourcesAsync(int courseId, int userId, bool isAdmin);
        Task<LearningResource?> SubmitAsync(LearningResource resource);
        Task<List<LearningResource>> GetPendingAsync();
        Task SetApprovalAsync(int resourceId, ApprovalStatus status);
        Task<bool> ToggleSaveAsync(int resourceId, int userId);
        Task<List<LearningResource>> GetSavedAsync(int userId);
        Task<LearningResource?> GetResourceAsync(int resourceId);
        Task<Course> AddCourseAsync(Course course);
    }

    public class LearnService : ILearnService
    {
        private readonly AppDbContext _context;

        public LearnService(AppDbContext context)
        {
            _context = context;
        }

        public Task<List<Course>> GetSemesterCoursesAsync(int semester)
        {
            return _context.Courses
                .Where(c => c.Track == CourseTrack.Semester && c.SemesterNumber == semester)
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        public Task<List<Course>> GetFundamentalCoursesAsync()
        {
            return _context.Courses
                .Where(c => c.Track == CourseTrack.Fundamental)
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        public Task<Course?> GetCourseAsync(int courseId)
        {
            return _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        }

        public Task<List<LearningResource>> GetVisibleResourcesAsync(int courseId, int userId, bool isAdmin)
        {
            return _context.LearningResources
                .Include(r => r.SubmittedBy)
                .Include(r => r.Saves)
                .Where(r => r.CourseId == courseId &&
                    (r.ApprovalStatus == ApprovalStatus.Approved || r.SubmittedByUserId == userId || isAdmin))
                .OrderByDescending(r => r.Year)
                .ThenBy(r => r.Title)
                .ToListAsync();
        }

        public async Task<LearningResource?> SubmitAsync(LearningResource resource)
        {
            resource.ApprovalStatus = ApprovalStatus.Pending;
            resource.DateCreated = DateTime.UtcNow;
            await _context.LearningResources.AddAsync(resource);
            await _context.SaveChangesAsync();
            return resource;
        }

        public Task<List<LearningResource>> GetPendingAsync()
        {
            return _context.LearningResources
                .Include(r => r.Course)
                .Include(r => r.SubmittedBy)
                .Where(r => r.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(r => r.DateCreated)
                .ToListAsync();
        }

        public async Task SetApprovalAsync(int resourceId, ApprovalStatus status)
        {
            var resource = await _context.LearningResources.FirstOrDefaultAsync(r => r.Id == resourceId);
            if (resource == null) return;
            resource.ApprovalStatus = status;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ToggleSaveAsync(int resourceId, int userId)
        {
            var existing = await _context.ResourceSaves
                .FirstOrDefaultAsync(s => s.LearningResourceId == resourceId && s.UserId == userId);
            if (existing != null)
            {
                _context.ResourceSaves.Remove(existing);
                await _context.SaveChangesAsync();
                return false;
            }

            await _context.ResourceSaves.AddAsync(new ResourceSave
            {
                LearningResourceId = resourceId,
                UserId = userId
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public Task<List<LearningResource>> GetSavedAsync(int userId)
        {
            return _context.ResourceSaves
                .Where(s => s.UserId == userId && s.LearningResource.ApprovalStatus == ApprovalStatus.Approved)
                .Include(s => s.LearningResource).ThenInclude(r => r.Course)
                .OrderByDescending(s => s.DateCreated)
                .Select(s => s.LearningResource)
                .ToListAsync();
        }

        public Task<LearningResource?> GetResourceAsync(int resourceId)
        {
            return _context.LearningResources
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.Id == resourceId);
        }

        public async Task<Course> AddCourseAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return course;
        }
    }
}
