using AustCseApp.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace AustCseApp.ViewModels.Home
{
    public class PostVM
    {
        public string? Content { get; set; }

        public PostKind PostKind { get; set; } = PostKind.General;

        [Required]
        public string? Batch { get; set; }

 
        public string? Tag { get; set; }

        public IFormFile? Image { get; set; }

        [StringLength(2048)]
        [Url(ErrorMessage = "Please provide a valid image URL.")]
        public string? ImageUrl { get; set; }
    }
}
