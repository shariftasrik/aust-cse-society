using AustCseApp.Data.Models;

namespace AustCseApp.ViewModels.Home
{
    public class BatchPageVM
    {
        public User Me { get; set; } = default!;
        public List<Post> Posts { get; set; } = new();
        public bool CanPin { get; set; }
    }
}
