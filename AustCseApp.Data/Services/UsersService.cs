using AustCseApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AustCseApp.Data.Services
{
    public class UsersService:IUsersService
    {
        private readonly AppDbContext _appDbContext;
        public UsersService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<User> GetUser(int userId)
        {
            return await _appDbContext.Users.FirstOrDefaultAsync(n => n.Id == userId);
        }
        public async Task<List<User>> GetOtherUsersAsync(int currentUserId, int take = 12)
        {
            return await _appDbContext.Users
                .Where(u => u.Id != currentUserId && !u.IsDeleted)
                .OrderBy(u => u.FullName)
                .Take(take)
                .ToListAsync();
        }
        public async Task UpdateUserProfilePicture(int loggedInUserId, string profilePictureUrl)
        {
            var userDb = await _appDbContext.Users.FirstOrDefaultAsync(n => n.Id == loggedInUserId);
            if (userDb != null)
            {
                userDb.ProfilePictureUrl = profilePictureUrl;
                _appDbContext.Users.Update(userDb);
                await _appDbContext.SaveChangesAsync();
            }
        }
    }
}
