using AustCseApp.Data.Helpers.Constants;
using AustCseApp.Data.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AustCseApp.Data.Helpers
{
    public class DbInitializer
    {
        public static async Task SeedUsersAndRolesAsync(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            //Roles
            if (!roleManager.Roles.Any())
            {
                foreach (var roleName in AppRoles.All)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                    }
                }
            }

            //Users with Roles
            if (!userManager.Users.Any(n => !string.IsNullOrEmpty(n.Email)))
            {
                var userPassword = "Coding@1234?";
                var newUser = new User()
                {
                    UserName = "nimurrahman",
                    Email = "nimur@gmail.com",
                    FullName = "Nimur Rahman Sharif",
                    ProfilePictureUrl = "https://pixabay.com/illustrations/man-male-cartoon-ai-generated-9637000/",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newUser, userPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(newUser, AppRoles.User);


                var newAdmin = new User()
                {
                    UserName = "admin.admin",
                    Email = "admin@tasrik.com",
                    FullName = "Tasrik Admin",
                    ProfilePictureUrl = "https://pixabay.com/illustrations/man-male-cartoon-ai-generated-9637000/",
                    EmailConfirmed = true
                };

                var resultNewAdmin = await userManager.CreateAsync(newAdmin, userPassword);
                if (resultNewAdmin.Succeeded)
                    await userManager.AddToRoleAsync(newAdmin, AppRoles.Admin);
            }
        }
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
                    Batch = "49",      
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
                    Batch = "49",
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
