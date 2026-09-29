namespace AustCseApp.Data.Models
{
    public class ReferralContact
    {
        public int Id { get; set; }
        public int ReferralRequestId { get; set; }
        public int AlumniUserId { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public ReferralRequest ReferralRequest { get; set; } = default!;
        public User Alumni { get; set; } = default!;
    }
}
