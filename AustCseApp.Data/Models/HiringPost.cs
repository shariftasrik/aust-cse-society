using System.ComponentModel.DataAnnotations;

namespace AustCseApp.Data.Models
{
    public class HiringPost
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(120)]
        public string RoleTitle { get; set; } = default!;

        [Required, StringLength(40)]
        public string JobType { get; set; } = "Full-time";

        [Required, StringLength(120)]
        public string Location { get; set; } = default!;

        [Required, StringLength(1000)]
        public string HowToApply { get; set; } = default!;

        public DateTime ClosingDate { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = default!;
    }
}
