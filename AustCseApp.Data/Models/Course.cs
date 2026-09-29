using System.ComponentModel.DataAnnotations;

namespace AustCseApp.Data.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string Code { get; set; } = default!;

        [Required, StringLength(160)]
        public string Title { get; set; } = default!;

        public CourseTrack Track { get; set; }

        public int? SemesterNumber { get; set; }

        [StringLength(80)]
        public string SyllabusEra { get; set; } = "Starter catalog";

        public ICollection<LearningResource> Resources { get; set; } = new List<LearningResource>();
    }
}
