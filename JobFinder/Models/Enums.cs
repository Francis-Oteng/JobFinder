using System.ComponentModel.DataAnnotations;

namespace JobFinder.Models
{
    public class Enums
    {
        public enum UserRole
        {
            Applicant = 0,
            Employer = 1,
            Admin = 2
        }

        public enum CompanySize
        {
            [Display(Name = "1-10 employees")]
            Micro = 0,

            [Display(Name = "11-50 employees")]
            Small = 1,

            [Display(Name = "51-200 employees")]
            Medium = 2,

            [Display(Name = "201-1000 employees")]
            Large = 3,

            [Display(Name = "1000+ employees")]
            Enterprise = 4
        }

        public enum ExperienceLevel
        {
            [Display(Name = "Entry Level")]
            Entry = 0,
            Junior = 1,
            [Display(Name = "Mid Level")]
            Mid = 2,
            Senior = 3,
            Lead = 4,
            Executive = 5
        }

        public enum JobType
        {
            [Display(Name = "Full Time")]
            FullTime = 0,

            [Display(Name = "Part Time")]
            PartTime = 1,

            Contract = 2,
            Internship = 3,
            Temporary = 4
        }

        public enum WorkArrangement
        {
            Onsite = 0,
            Remote = 1,
            Hybrid = 2
        }

        public enum JobStatus
        {
            Draft = 0,
            Active = 1,
            Closed = 2,
            Expired = 3
        }

        public enum ApplicationStatus
        {
            Pending = 0,

            [Display(Name = "Under Review")]
            UnderReview = 1,

            Shortlisted = 2,
            Interview = 3,
            Accepted = 4,
            Rejected = 5,
            Withdrawn = 6
        }

        public enum InterviewType
        {
            [Display(Name = "In Person")]
            InPerson = 0,

            Online = 1,
            Phone = 2
        }

        public enum InterviewStatus
        {
            Scheduled = 0,
            Completed = 1,
            Cancelled = 2,
            Rescheduled = 3
        }

    }
}
