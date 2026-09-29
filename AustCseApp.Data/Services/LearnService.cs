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
        Task<List<Course>> GetAllCoursesAsync();
        Task<bool> UpdateCourseAsync(int id, string code, string title, int? semester, CourseTrack track);
        Task<bool> DeleteCourseAsync(int id);
        Task<bool> UpdateResourceAsync(int id, string title, string? sessionLabel, int? year);
        Task<bool> DeleteResourceAsync(int id);
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

        public Task<List<Course>> GetAllCoursesAsync()
        {
            return _context.Courses
                .Include(c => c.Resources)
                .OrderBy(c => c.Track)
                .ThenBy(c => c.SemesterNumber)
                .ThenBy(c => c.Code)
                .ToListAsync();
        }

        public async Task<bool> UpdateCourseAsync(int id, string code, string title, int? semester, CourseTrack track)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title)) return false;
            course.Code = code.Trim();
            course.Title = title.Trim();
            course.Track = track;
            course.SemesterNumber = track == CourseTrack.Fundamental ? null : semester;
            if (track == CourseTrack.Fundamental) course.SyllabusEra = "Career";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return false;
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateResourceAsync(int id, string title, string? sessionLabel, int? year)
        {
            var resource = await _context.LearningResources.FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null || string.IsNullOrWhiteSpace(title)) return false;
            resource.Title = title.Trim();
            resource.SessionLabel = string.IsNullOrWhiteSpace(sessionLabel) ? null : sessionLabel.Trim();
            resource.Year = year;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteResourceAsync(int id)
        {
            var resource = await _context.LearningResources.FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null) return false;
            _context.LearningResources.Remove(resource);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
