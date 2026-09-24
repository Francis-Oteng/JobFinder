
using System.ComponentModel.DataAnnotations;
using static JobFinder.Models.Enums;

namespace JobFinder.Models.ViewModels
{
    public class ApplicantProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Bio")]
        [DataType(DataType.MultilineText)]
        public string? Bio { get; set; }

        [Display(Name = "Skills")]
        public string? Skills { get; set; }

        [Display(Name = "Education")]
        [DataType(DataType.MultilineText)]
        public string? Education { get; set; }

        [Display(Name = "Certifications")]
        [DataType(DataType.MultilineText)]
        public string? Certifications { get; set; }

        [Display(Name = "Experience Level")]
        public ExperienceLevel? ExperienceLevel { get; set; }

        [Display(Name = "Years of Experience")]
        [Range(0, 60, ErrorMessage = "Experience must be between 0 and 60 years.")]
        public int? ExperienceYears { get; set; }

        [Display(Name = "Preferred Job Type")]
        public JobType? PreferredJobType { get; set; }

        [Display(Name = "Resume URL")]
        [Url(ErrorMessage = "Please enter a valid resume URL.")]
        public string? ResumeUrl { get; set; }

        [Display(Name = "Portfolio URL")]
        [Url(ErrorMessage = "Please enter a valid portfolio URL.")]
        public string? PortfolioUrl { get; set; }

        [Display(Name = "Profile Photo URL")]
        [Url(ErrorMessage = "Please enter a valid photo URL.")]
        public string? PhotoUrl { get; set; }
    }
}
