using AustCseApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AustCseApp.Data.Helpers
{
    public class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext appDbContext)
        {
            if (!appDbContext.Users.Any() && !appDbContext.Posts.Any())
            {
                var newUser = new User()
                {
                    FullName = "Nimur Rahman",
                    ProfilePictureUrl = "https://www.freepik.com/free-vector/blue-circle-with-white-user_145857007.htm#fromView=keyword&page=1&position=0&uuid=09957629-0fb6-41b9-8eb4-e0e1c63e28f1&query=Profile",
                };
                await appDbContext.Users.AddAsync(newUser);
                await appDbContext.SaveChangesAsync();


                var newPostWithoutImage = new Post()
                {
                    Content = "First post loaded from database",
                    Batch = "CSE 49",      
                    Tag = "Q&A",         
                    ImageUrl = null,          
                    NrOfReports = 0,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow,


                    UserId = newUser.Id,
                };

                var newPostWithImage = new Post
                {
                    Content = "Seeded post with an image",
                    Batch = "CSE 49",
                    Tag = "Project",
                    ImageUrl = "https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&w=1600&q=80",
                    NrOfReports = 0,
                    DateCreated = DateTime.UtcNow,
                    DateUpdated = DateTime.UtcNow,


                    UserId = newUser.Id,
                };


                
                await appDbContext.Posts.AddRangeAsync(newPostWithoutImage, newPostWithImage);
                await appDbContext.SaveChangesAsync();
            }
        }
    }
}
