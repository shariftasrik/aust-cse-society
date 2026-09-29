namespace AustCseApp.ViewModels.Settings
{
    public class UpdateProfileVM
    {
        public string FullName { get; set; }

        // public string EmailAddress { get; set; }

        public string UserName { get; set; }
        public string Bio { get; set; }
        public int? CurrentSemester { get; set; }
        public string? Company { get; set; }
        public string? JobTitle { get; set; }
        public bool CanRefer { get; set; }
        public string? Batch { get; set; }
    }
}
