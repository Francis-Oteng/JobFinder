using static JobFinder.Models.Enums;

namespace JobFinder.Models.ViewModels
{
    public class JobDetailsViewModel
    {
        public int JobId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public JobType? JobType { get; set; }

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public DateTime PostedDate { get; set; }

        public DateTime ClosingDate { get; set; }

        // Company information
        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyDescription { get; set; }

        public string? CompanyLogoUrl { get; set; }

        public string? CompanyWebsite { get; set; }

        // Applicant state
        public bool IsSaved { get; set; }

        public bool HasApplied { get; set; }
    }
}

