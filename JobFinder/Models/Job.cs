using static JobFinder.Models.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace JobFinder.Models
{
    public class Job
    {
        public int JobId { get; set; }

        public int EmployerId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        /// <summary>Renamed from "Requirement" — a job has many requirements.</summary>
        public string Requirements { get; set; } = null!;

        public string? Responsibilities { get; set; }

        public string? Benefits { get; set; }

        /// <summary>Comma-separated. Consumed by the AI matching service.</summary>
        public string? RequiredSkills { get; set; }

        public string? Education { get; set; }

        public ExperienceLevel? ExperienceLevel { get; set; }

        public int? ExperienceYears { get; set; }

        public string? Category { get; set; }

        public string Location { get; set; } = null!;

        public JobType JobType { get; set; } = JobType.FullTime;

        public WorkArrangement WorkArrangement { get; set; } = WorkArrangement.Onsite;

        /// <summary>Nullable: some postings genuinely do not disclose pay.</summary>
        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public JobStatus Status { get; set; } = JobStatus.Draft;

        public DateTime PostedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ClosingDate { get; set; }

        // ---- Navigation ----

        public virtual Employer Employer { get; set; } = null!;

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

        public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
