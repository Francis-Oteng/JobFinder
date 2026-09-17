using static JobFinder.Models.Enums;

namespace JobFinder.Models
{
    public class Application
    {

        public int ApplicationId { get; set; }

        public int ApplicantId { get; set; }

        public int JobId { get; set; }

        public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        /// <summary>
        /// Nullable on purpose: the application row is created first,
        /// the AI matching service fills this in afterwards. Range 0-100.
        /// </summary>
        public int? AIMatchScore { get; set; }

        public string? CoverLetter { get; set; }

        /// <summary>
        /// Snapshot of the resume used for THIS application.
        /// Kept separate from Applicant.ResumeUrl so later profile edits
        /// don't rewrite history.
        /// </summary>
        public string? ResumeUrl { get; set; }

        /// <summary>Set the first time an employer moves it out of Pending.</summary>
        public DateTime? ReviewedDate { get; set; }

        /// <summary>Private notes — employer-side only, never shown to the applicant.</summary>
        public string? EmployerNotes { get; set; }

        // ---- Navigation ----

        public virtual Applicant Applicant { get; set; } = null!;

        public virtual Job Job { get; set; } = null!;

        public virtual ICollection<Interview> Interviews { get; set; } = new List<Interview>();

        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
}
