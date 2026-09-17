using static JobFinder.Models.Enums;

namespace JobFinder.Models
{
    public class ApplicationUser
    {

        public int UserId { get; set; }

        public string FullName { get; set; } = null!;

        /// <summary>Unique. Used as the login identifier.</summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// NEVER a plaintext password. Store a BCrypt/Argon2 hash here
        /// (BCrypt embeds its own salt, so no separate salt column is needed).
        /// </summary>
        public string PasswordHash { get; set; } = null!;

        public UserRole Role { get; set; }

        /// <summary>Soft-disable an account instead of deleting it.</summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginDate { get; set; }

        // ---- Navigation ----

        /// <summary>0..1 — only populated when Role == Applicant.</summary>
        public virtual Applicant? Applicant { get; set; }

        /// <summary>0..1 — only populated when Role == Employer.</summary>
        public virtual Employer? Employer { get; set; }

        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();

        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

    }
}
