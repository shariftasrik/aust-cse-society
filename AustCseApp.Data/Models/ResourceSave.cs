namespace AustCseApp.Data.Models
{
    public class ResourceSave
    {
        public int UserId { get; set; }
        public int LearningResourceId { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = default!;
        public LearningResource LearningResource { get; set; } = default!;
    }
}
