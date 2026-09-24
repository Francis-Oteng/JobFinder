using System.ComponentModel.DataAnnotations;
using static JobFinder.Models.Enums;

namespace JobFinder.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = null!;

        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = null!;

        [Required, DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public UserRole Role { get; set; } = UserRole.Applicant;

        // ---- Applicant-only (optional at signup, can complete later) ----
        [Display(Name = "Location")]
        public string? ApplicantLocation { get; set; }

        [Display(Name = "Phone number")]
        public string? ApplicantPhoneNumber { get; set; }

        // ---- Employer-only (required if Role == Employer) ----
        [Display(Name = "Company name")]
        public string? CompanyName { get; set; }

        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [Display(Name = "Company location")]
        public string? EmployerLocation { get; set; }

        [Display(Name = "Company phone")]
        public string? EmployerPhoneNumber { get; set; }
    }
}
