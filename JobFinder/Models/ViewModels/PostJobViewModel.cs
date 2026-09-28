
using JobFinder.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using static JobFinder.Models.Enums;

namespace JobFinder.Models.ViewModels
{
    public class PostJobViewModel
    {
        public int? JobId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string Requirements { get; set; } = string.Empty;

        public string? Responsibilities { get; set; }

        public string? Benefits { get; set; }

        [Display(Name = "Required Skills")]
        public string? RequiredSkills { get; set; }

        public string? Education { get; set; }

        [Display(Name = "Experience Level")]
        public ExperienceLevel? ExperienceLevel { get; set; }

        [Range(0, 60)]
        [Display(Name = "Years of Experience")]
        public int? ExperienceYears { get; set; }

        public string? Category { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Job Type")]
        public JobType JobType { get; set; } = JobType.FullTime;

        [Required]
        [Display(Name = "Work Arrangement")]
        public WorkArrangement WorkArrangement { get; set; } = WorkArrangement.Onsite;

        [Range(0, 100000000)]
        [Display(Name = "Minimum Salary")]
        public decimal? SalaryMin { get; set; }

        [Range(0, 100000000)]
        [Display(Name = "Maximum Salary")]
        public decimal? SalaryMax { get; set; }

        [Required]
        public JobStatus Status { get; set; } = JobStatus.Draft;

        [DataType(DataType.Date)]
        [Display(Name = "Closing Date")]
        public DateTime? ClosingDate { get; set; }


        // ============================================================
        // SELECT LIST OPTIONS
        // ============================================================

        public List<SelectListItem> JobTypeOptions { get; set; } = new();

        public List<SelectListItem> WorkArrangementOptions { get; set; } = new();

        public List<SelectListItem> ExperienceLevelOptions { get; set; } = new();

        public List<SelectListItem> StatusOptions { get; set; } = new();
    }
}

