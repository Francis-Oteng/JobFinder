using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using static JobFinder.Models.Enums;

namespace JobFinder.Models.ViewModels
{
    public class PostJobViewModel
    {
        public int JobId { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = null!;

        [Required, DataType(DataType.MultilineText)]
        public string Description { get; set; } = null!;

        [Required, DataType(DataType.MultilineText)]
        public string Requirements { get; set; } = null!;

        [DataType(DataType.MultilineText)]
        public string? Responsibilities { get; set; }

        [StringLength(2000)]
        public string? Benefits { get; set; }

        [StringLength(1000)]
        [Display(Name = "Required skills")]
        public string? RequiredSkills { get; set; }

        [StringLength(500)]
        public string? Education { get; set; }

        [Display(Name = "Experience level")]
        public ExperienceLevel? ExperienceLevel { get; set; }

        [Range(0, 60)]
        [Display(Name = "Years of experience")]
        public int? ExperienceYears { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        [Required, StringLength(150)]
        public string Location { get; set; } = null!;

        [Required]
        [Display(Name = "Job type")]
        public JobType JobType { get; set; } = JobType.FullTime;

        [Required]
        [Display(Name = "Work arrangement")]
        public WorkArrangement WorkArrangement { get; set; } = WorkArrangement.Onsite;

        [Range(0, 100_000_000)]
        [Display(Name = "Minimum salary")]
        public decimal? SalaryMin { get; set; }

        [Range(0, 100_000_000)]
        [Display(Name = "Maximum salary")]
        public decimal? SalaryMax { get; set; }

        [Required]
        public JobStatus Status { get; set; } = JobStatus.Draft;

        [DataType(DataType.Date)]
        [Display(Name = "Closing date")]
        public DateTime? ClosingDate { get; set; }

        // ---- Populated by the controller for the dropdowns ----
        public List<SelectListItem> JobTypeOptions { get; set; } = new();
        public List<SelectListItem> WorkArrangementOptions { get; set; } = new();
        public List<SelectListItem> ExperienceLevelOptions { get; set; } = new();
        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}
