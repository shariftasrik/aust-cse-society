using AustCseApp.Data.Models;

namespace AustCseApp.ViewModels.Users
{
    public class UserProfileVM
    {
        public User ProfileUser { get; set; }
        public List<Post> Posts { get; set; } = new();
    }
}
