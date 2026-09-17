using static JobFinder.Models.Enums;

namespace JobFinder.Models
{
    public class Employer
    {
        public int EmployerId { get; set; }

        /// <summary>FK to the account. Unique index enforces the 1 : 0..1 relationship.</summary>
        public int UserId { get; set; }

        public string CompanyName { get; set; } = null!;

        public string Industry { get; set; } = null!;

        public string Location { get; set; } = null!;

        public string? Website { get; set; }

        public string? PhoneNumber { get; set; }

        public CompanySize? CompanySize { get; set; }

        public string? Description { get; set; }

        public string? LogoUrl { get; set; }

        /// <summary>Set by an Admin after the company is vetted.</summary>
        public bool IsVerified { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ---- Navigation ----

        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();

    }
}
