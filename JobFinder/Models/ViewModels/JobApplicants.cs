using JobFinder.Models;
using static JobFinder.Models.Enums;

namespace JobFinder.Models.ViewModels
{
    public class JobApplicantsViewModel
    {
        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public int TotalApplicants { get; set; }

        public List<JobApplicantRowViewModel> Applicants { get; set; }
            = new List<JobApplicantRowViewModel>();
    }

    public class JobApplicantRowViewModel
    {
        public int ApplicationId { get; set; }
        public int ApplicantId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? Skills { get; set; }
        public string? Education { get; set; }
        public string? ExperienceLevel { get; set; }
        public int? ExperienceYears { get; set; }
        public int? AIMatchScore { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string? ResumeUrl { get; set; }
        public string? CoverLetter { get; set; }
        public DateTime? ReviewedDate { get; set; }

        public string JobTitle { get; set; } = string.Empty;
    }

}