using static JobFinder.Models.Enums;

namespace JobFinder.Models.ViewModels
{
    public class ApplicantJobsViewModel
    {
        public string? Search { get; set; }

        public JobType? JobType { get; set; }

        public string? Location { get; set; }

        public string? Category { get; set; }

        public List<JobOpportunityViewModel> Jobs { get; set; }
            = new List<JobOpportunityViewModel>();
    }

    public class JobOpportunityViewModel
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

        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyLogoUrl { get; set; }

        public bool IsSaved { get; set; }

        public bool HasApplied { get; set; }
    }
}