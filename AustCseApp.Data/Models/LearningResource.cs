using System.ComponentModel.DataAnnotations;

namespace AustCseApp.Data.Models
{
    public class LearningResource
    {
        public int Id { get; set; }
        public int CourseId { get; set; }

        public ResourceKind Kind { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; } = default!;

        [StringLength(500)]
        public string? YoutubeUrl { get; set; }

        [StringLength(300)]
        public string? FilePath { get; set; }

        [StringLength(40)]
        public string? SessionLabel { get; set; }

        public int? Year { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public bool IsOriginalOrOpenLicense { get; set; }

        public int SubmittedByUserId { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public Course Course { get; set; } = default!;
        public User SubmittedBy { get; set; } = default!;
        public ICollection<ResourceSave> Saves { get; set; } = new List<ResourceSave>();
    }
}
