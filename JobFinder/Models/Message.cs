using static System.Net.Mime.MediaTypeNames;

namespace JobFinder.Models
{
    public class Message
    {

        public int MessageId { get; set; }

        public int SenderId { get; set; }

        public int RecipientId { get; set; }

        public string Content { get; set; } = null!;

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }

        /// <summary>Optional context.</summary>
        public int? JobId { get; set; }

        /// <summary>Optional context.</summary>
        public int? ApplicationId { get; set; }

        // ---- Navigation ----

        public virtual ApplicationUser Sender { get; set; } = null!;

        public virtual ApplicationUser Recipient { get; set; } = null!;

        public virtual Job? Job { get; set; }

        public virtual Application? Application { get; set; }

    }
}
