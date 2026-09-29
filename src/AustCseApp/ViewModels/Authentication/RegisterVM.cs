using AustCseApp.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace AustCseApp.ViewModels.Authentication
{
    public class RegisterVM
    {
        public AccountKind AccountKind { get; set; } = AccountKind.CurrentStudent;

        [Required(ErrorMessage = "Batch is required")]
        [RegularExpression(@"^\d{2,3}$", ErrorMessage = "Batch should look like 49")]
        public string Batch { get; set; }

        public string? StudentId { get; set; }
        public int? CurrentSemester { get; set; }
        public int? GraduationYear { get; set; }
        public string? Company { get; set; }
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First Name must contain only letters")]
        public string FirstName { get; set; }


        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last Name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last Name must contain only letters")]
        public string LastName { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}