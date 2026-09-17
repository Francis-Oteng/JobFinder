using static JobFinder.Models.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace JobFinder.Models
{
    public class Applicant
    {

        public int ApplicantId { get; set; }

        /// <summary>FK to the account. Unique index enforces the 1 : 0..1 relationship.</summary>
        public int UserId { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Location { get; set; }

        public string? Bio { get; set; }

        /// <summary>Comma-separated skills, e.g. "C#, SQL, Azure". Kept flat deliberately.</summary>
        public string? Skills { get; set; }

        public string? Education { get; set; }

        public string? Certifications { get; set; }

        public ExperienceLevel? ExperienceLevel { get; set; }

        public int? ExperienceYears { get; set; }

        public JobType? PreferredJobType { get; set; }

        public string? ResumeUrl { get; set; }

        public string? PortfolioUrl { get; set; }

        public string? PhotoUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        // ---- Navigation ----

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

        public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();

    }
}
