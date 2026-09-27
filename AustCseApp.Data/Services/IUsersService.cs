using AustCseApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AustCseApp.Data.Services
{
    public interface IUsersService
    {
        Task<User> GetUser(int userId);
        Task<List<User>> GetOtherUsersAsync(int currentUserId, int take = 12);
        Task UpdateUserProfilePicture(int loggedInUserId, string profilePictureUrl);
    }
}
