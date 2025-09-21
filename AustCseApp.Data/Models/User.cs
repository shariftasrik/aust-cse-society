using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AustCseApp.Data.Models
{
    public class User : IdentityUser<int>
    {
        //public int Id { get; set; }
        public string FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }

        public bool IsDeleted { get; set; }


        // Navigation property, one to many maintain korar jonno
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
