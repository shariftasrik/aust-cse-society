namespace AustCseApp.Data.Models
{
    public enum AccountKind
    {
        CurrentStudent = 0,
        Alumni = 1
    }

    public enum VerificationStatus
    {
        Pending = 0,
        Verified = 1,
        Rejected = 2
    }

    public enum PostKind
    {
        Notice = 0,
        Question = 1,
        ResourceTip = 2,
        General = 3
    }

    public enum CourseTrack
    {
        Semester = 0,
        Fundamental = 1
    }

    public enum ResourceKind
    {
        Video = 0,
        Quiz = 1,
        Final = 2,
        Note = 3,
        Book = 4
    }

    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
