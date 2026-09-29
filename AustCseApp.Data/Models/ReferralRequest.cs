using System.ComponentModel.DataAnnotations;

namespace AustCseApp.Data.Models
{
    public class ReferralRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, StringLength(120)]
        public string TargetRole { get; set; } = default!;

        [Required, StringLength(300)]
        public string Skills { get; set; } = default!;

        [Required, StringLength(2000)]
        public string Note { get; set; } = default!;

        [StringLength(300)]
        public string? CvPath { get; set; }

        public bool IsOpen { get; set; } = true;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = default!;
        public ICollection<ReferralContact> Contacts { get; set; } = new List<ReferralContact>();
    }
}
