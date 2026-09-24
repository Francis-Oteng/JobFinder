using static JobFinder.Models.Enums;
namespace JobFinder.Models.ViewModels
{ public class ApplicantDashboardViewModel 
    { public string FullName { get; set; } = string.Empty; 
        public bool ProfileComplete { get; set; } 
        public int ApplicationsCount { get; set; } 
        public int ShortlistedCount { get; set; } 
        public int InterviewCount { get; set; } 
        public int SavedJobsCount { get; set; } 
        public List<RecentApplicationRow> RecentApplications { get; set; } = new(); } 
    public class RecentApplicationRow { public int ApplicationId { get; set; }
        public int JobId { get; set; } 
        public string JobTitle { get; set; } = string.Empty; 
        public string CompanyName { get; set; } = string.Empty;
        public ApplicationStatus Status { get; set; } 
        public DateTime ApplicationDate { get; set; } } }