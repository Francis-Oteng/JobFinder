using static JobFinder.Models.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace JobFinder.Models
{
    public class Interview
    {
        public int InterviewId { get; set; }

        public int ApplicationId { get; set; }

        public DateTime InterviewDate { get; set; }

        public InterviewType InterviewType { get; set; } = InterviewType.Online;

        /// <summary>Video link for Online, or the physical address / phone number otherwise.</summary>
        public string? MeetingLink { get; set; }

        public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ---- Navigation ----

        public virtual Application Application { get; set; } = null!;

    }
}
