using AustCseApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AustCseApp.Data.Services
{
    public interface ICareerService
    {
        Task<ReferralRequest?> GetOpenRequestAsync(int userId);
        Task<List<ReferralRequest>> GetOpenRequestsAsync();
        Task<ReferralRequest?> GetRequestAsync(int id);
        Task<bool> CreateRequestAsync(ReferralRequest request);
        Task<bool> AskForCvAsync(int requestId, int alumniUserId);
        Task CloseRequestAsync(int requestId, int userId);
        Task<List<HiringPost>> GetOpenHiringAsync();
        Task CreateHiringAsync(HiringPost post);
    }

    public class CareerService : ICareerService
    {
        private readonly AppDbContext _context;

        public CareerService(AppDbContext context)
        {
            _context = context;
        }

        public Task<ReferralRequest?> GetOpenRequestAsync(int userId)
        {
            return _context.ReferralRequests
                .Include(r => r.User)
                .Include(r => r.Contacts).ThenInclude(c => c.Alumni)
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsOpen);
        }

        public Task<List<ReferralRequest>> GetOpenRequestsAsync()
        {
            return _context.ReferralRequests
                .Include(r => r.User)
                .Include(r => r.Contacts)
                .Where(r => r.IsOpen)
                .OrderByDescending(r => r.DateCreated)
                .ToListAsync();
        }

        public Task<ReferralRequest?> GetRequestAsync(int id)
        {
            return _context.ReferralRequests
                .Include(r => r.User)
                .Include(r => r.Contacts)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<bool> CreateRequestAsync(ReferralRequest request)
        {
            var hasOpen = await _context.ReferralRequests.AnyAsync(r => r.UserId == request.UserId && r.IsOpen);
            if (hasOpen) return false;

            request.IsOpen = true;
            request.DateCreated = DateTime.UtcNow;
            await _context.ReferralRequests.AddAsync(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AskForCvAsync(int requestId, int alumniUserId)
        {
            var request = await _context.ReferralRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.IsOpen);
            if (request == null || request.UserId == alumniUserId) return false;

            var already = await _context.ReferralContacts.AnyAsync(c =>
                c.ReferralRequestId == requestId && c.AlumniUserId == alumniUserId);
            if (already) return true;

            await _context.ReferralContacts.AddAsync(new ReferralContact
            {
                ReferralRequestId = requestId,
                AlumniUserId = alumniUserId
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CloseRequestAsync(int requestId, int userId)
        {
            var request = await _context.ReferralRequests.FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId);
            if (request == null) return;
            request.IsOpen = false;
            await _context.SaveChangesAsync();
        }

        public Task<List<HiringPost>> GetOpenHiringAsync()
        {
            var today = DateTime.UtcNow.Date;
            return _context.HiringPosts
                .Include(h => h.User)
                .Where(h => h.ClosingDate.Date >= today)
                .OrderBy(h => h.ClosingDate)
                .ToListAsync();
        }

        public async Task CreateHiringAsync(HiringPost post)
        {
            post.DateCreated = DateTime.UtcNow;
            await _context.HiringPosts.AddAsync(post);
            await _context.SaveChangesAsync();
        }
    }
}
