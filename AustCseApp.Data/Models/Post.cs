using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
//using Microsoft.AspNetCore.Http;

namespace AustCseApp.Data.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = default!;


        [StringLength(32)]
        [RegularExpression(@"^\d{1,3}$",
            ErrorMessage = "Batch Number should input like '49' ")]
        public string Batch { get; set; }

        [StringLength(50)]
        public string Tag { get; set; }

        [StringLength(2048)]
        public string ImageUrl { get; set; }
        public bool IsPrivate { get; set; }

        [NotMapped]
        //public IFormFile? UploadedContent { get; set; }

        [Range(0, int.MaxValue)]

        public int NrOfReports { get; set; } = 0;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; }


        // Foreign Key
        public int UserId { get; set; }

        // Navigation Properties
        public User User { get; set; }
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
