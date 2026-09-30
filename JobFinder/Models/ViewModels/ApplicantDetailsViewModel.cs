using static JobFinder.Models.Enums;

namespace JobFinder.Models.ViewModels
{
    public class ApplicantDetailsViewModel
    {
        // Applicant
        public int ApplicantId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string? Location { get; set; }

        public string? Bio { get; set; }

        public string? Skills { get; set; }

        public string? Education { get; set; }

        public string? Certifications { get; set; }

        public ExperienceLevel? ExperienceLevel { get; set; }

        public int? ExperienceYears { get; set; }

        public string? ResumeUrl { get; set; }

        public string? PortfolioUrl { get; set; }

        public string? PhotoUrl { get; set; }


        // Application
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public DateTime ApplicationDate { get; set; }

        public ApplicationStatus Status { get; set; }

        public int? AIMatchScore { get; set; }

        public string? CoverLetter { get; set; }

        public DateTime? ReviewedDate { get; set; }

        public string? EmployerNotes { get; set; }
    }
}