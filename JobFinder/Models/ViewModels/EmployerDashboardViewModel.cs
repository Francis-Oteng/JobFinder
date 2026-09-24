using JobFinder.Controllers;
using JobFinder.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using static JobFinder.Models.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace JobFinder.Models.ViewModels
{
    public class EmployerDashboardViewModel
    {
        public string CompanyName { get; set; } = string.Empty;

        public string? CompanyLogoUrl { get; set; }

        public bool IsVerified { get; set; }

        public bool ProfileComplete { get; set; }

        public int JobsCount { get; set; }

        public int ActiveJobsCount { get; set; }

        public int DraftJobsCount { get; set; }

        public int ApplicationsCount { get; set; }

        public int TotalApplicantsCount { get; set; }

        public int ShortlistedCount { get; set; }

        public int SubmittedCount { get; set; }

        public int WithdrawnCount { get; set; }

        public int InterviewCount { get; set; }

        public int    PendingReviewCount { get; set; }


        public List<RecentJobRow> RecentJobs { get; set; } = new();

        public List<EmployerApplicationRow> RecentApplications { get; set; } = new();
    }

    public class RecentJobRow
    {
        public int JobId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public JobType? JobType { get; set; }

        public DateTime PostedDate { get; set; }

        public DateTime ClosingDate { get; set; }

        public int ApplicationsCount { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    public class EmployerApplicationRow
    {
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string ApplicantName { get; set; } = string.Empty;

        public ApplicationStatus Status { get; set; }

        public DateTime ApplicationDate { get; set; }
    }
}
