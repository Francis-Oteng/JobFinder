namespace JobFinder.Models
{
    public class SavedJob
    {

        public int SavedJobId { get; set; }

        public int ApplicantId { get; set; }

        public int JobId { get; set; }

        public DateTime SavedDate { get; set; } = DateTime.UtcNow;

        // ---- Navigation ----

        public virtual Applicant Applicant { get; set; } = null!;

        public virtual Job Job { get; set; } = null!;

    }
}
