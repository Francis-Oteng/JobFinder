
using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Models.ViewModels;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static JobFinder.Models.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace JobFinder.Controllers
{
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployerController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // DASHBOARD
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var employer = await _context.Employers
                .Include(e => e.Jobs)
                    .ThenInclude(j => j.Applications)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            var vm = new EmployerDashboardViewModel
            {
                CompanyName = employer.CompanyName,
                IsVerified = employer.IsVerified,

                // Job statistics
                ActiveJobsCount = employer.Jobs
                    .Count(j => j.Status == JobStatus.Active),

                DraftJobsCount = employer.Jobs
                    .Count(j => j.Status == JobStatus.Draft),

                // Application statistics
                TotalApplicantsCount = employer.Jobs
                    .Sum(j => j.Applications.Count),

                ShortlistedCount = employer.Jobs
                    .SelectMany(j => j.Applications)
                    .Count(a => a.Status == ApplicationStatus.Shortlisted),

                InterviewCount = employer.Jobs
                    .SelectMany(j => j.Applications)
                    .Count(a => a.Status == ApplicationStatus.Interview),

                SubmittedCount = employer.Jobs
                    .SelectMany(j => j.Applications)
                    .Count(a => a.Status == ApplicationStatus.Submitted),

                WithdrawnCount = employer.Jobs
                    .SelectMany(j => j.Applications)
                    .Count(a => a.Status == ApplicationStatus.Withdrawn),

                PendingReviewCount = employer.Jobs
                    .SelectMany(j => j.Applications)
                    .Count(a => a.Status == ApplicationStatus.Pending),

                // Recent jobs
                RecentJobs = employer.Jobs
                    .OrderByDescending(j => j.PostedDate)
                    .Take(5)
                    .Select(j => new RecentJobRow
                    {
                        JobId = j.JobId,
                        Title = j.Title,
                        Status = j.Status.ToString(),
                        Category = j.Category,
                        Location = j.Location,
                        ApplicationsCount = j.Applications.Count,
                        PostedDate = j.PostedDate
                    })
                    .ToList()
            };

            return View(vm);
        }


        // ============================================================
        // POST JOB - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> PostJob()
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            var model = new PostJobViewModel();

            PopulateSelectLists(model);

            return View(model);
        }


        // ============================================================
        // POST JOB - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(PostJobViewModel model)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            // Validate salary range
            if (model.SalaryMin.HasValue &&
                model.SalaryMax.HasValue &&
                model.SalaryMax < model.SalaryMin)
            {
                ModelState.AddModelError(
                    nameof(model.SalaryMax),
                    "Maximum salary must be at least the minimum salary.");
            }

            // Validate closing date
            if (model.ClosingDate.HasValue &&
                model.ClosingDate.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(
                    nameof(model.ClosingDate),
                    "Closing date can't be in the past.");
            }

            // Only Draft and Active are allowed when creating a job
            if (model.Status != JobStatus.Draft &&
                model.Status != JobStatus.Active)
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "New postings must be Draft or Active.");
            }

            if (!ModelState.IsValid)
            {
                PopulateSelectLists(model);
                return View(model);
            }

            var job = new Job
            {
                EmployerId = employer.EmployerId,

                Title = model.Title,
                Description = model.Description,
                Requirements = model.Requirements,
                Responsibilities = model.Responsibilities,
                Benefits = model.Benefits,
                RequiredSkills = model.RequiredSkills,
                Education = model.Education,

                ExperienceLevel = model.ExperienceLevel,
                ExperienceYears = model.ExperienceYears,

                Category = model.Category,
                Location = model.Location,

                JobType = (JobType)model.JobType,
                WorkArrangement = (WorkArrangement)model.WorkArrangement,

                SalaryMin = model.SalaryMin,
                SalaryMax = model.SalaryMax,

                Status = model.Status,

                ClosingDate = model.ClosingDate,

                PostedDate = DateTime.UtcNow
            };

            _context.Jobs.Add(job);

            await _context.SaveChangesAsync();

            TempData["Success"] = model.Status == JobStatus.Active
                ? $"\"{job.Title}\" was posted and is now live."
                : $"\"{job.Title}\" was saved as a draft.";

            return RedirectToAction(nameof(Dashboard));
        }


        // ============================================================
        // JOBS - GET
        // Displays all jobs belonging to the logged-in employer
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Jobs(
            string? search,
            string? status,
            string? category)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            var query = _context.Jobs
                .Where(j => j.EmployerId == employer.EmployerId)
                .Include(j => j.Applications)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(j =>
                    j.Title.Contains(search) ||
                    j.Description.Contains(search) ||
                    (j.Category != null &&
                     j.Category.Contains(search)));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();

                query = query.Where(j =>
                    j.Status.ToString() == status);
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                category = category.Trim();

                query = query.Where(j =>
                    j.Category != null &&
                    j.Category.Contains(category));
            }

            var jobs = await query
                .OrderByDescending(j => j.PostedDate)
                .ToListAsync();

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Category = category;

            return View(jobs);
        }


        // ============================================================
        // EDIT JOB - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            // IMPORTANT:
            // The EmployerId condition prevents one employer
            // from editing another employer's job.
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employer.EmployerId);

            if (job is null)
            {
                return NotFound(
                    "Job posting not found or you do not have permission to edit it.");
            }

            // Reuse PostJobViewModel for editing
            var model = new PostJobViewModel
            {
                JobId = job.JobId,

                Title = job.Title,
                Description = job.Description,
                Requirements = job.Requirements,
                Responsibilities = job.Responsibilities,
                Benefits = job.Benefits,
                RequiredSkills = job.RequiredSkills,
                Education = job.Education,

                ExperienceLevel = job.ExperienceLevel,
                ExperienceYears = job.ExperienceYears,

                Category = job.Category,
                Location = job.Location,

                JobType = job.JobType,
                WorkArrangement = job.WorkArrangement,

                SalaryMin = job.SalaryMin,
                SalaryMax = job.SalaryMax,

                Status = job.Status,

                ClosingDate = job.ClosingDate
            };

            PopulateSelectLists(model);

            return View(model);
        }


        // ============================================================
        // EDIT JOB - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJob(
            int id,
            PostJobViewModel model)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            // Retrieve only jobs belonging to this employer
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employer.EmployerId);

            if (job is null)
            {
                return NotFound(
                    "Job posting not found or you do not have permission to edit it.");
            }

            // ========================================================
            // VALIDATION
            // ========================================================

            // Salary validation
            if (model.SalaryMin.HasValue &&
                model.SalaryMax.HasValue &&
                model.SalaryMax < model.SalaryMin)
            {
                ModelState.AddModelError(
                    nameof(model.SalaryMax),
                    "Maximum salary must be at least the minimum salary.");
            }

            // Closing date validation
            if (model.ClosingDate.HasValue &&
                model.ClosingDate.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(
                    nameof(model.ClosingDate),
                    "Closing date can't be in the past.");
            }

            // Validate enum value
            if (!Enum.IsDefined(typeof(JobStatus), model.Status))
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "Invalid job status.");
            }

            if (!ModelState.IsValid)
            {
                PopulateSelectLists(model);
                return View(model);
            }

            // ========================================================
            // UPDATE JOB
            // ========================================================

            job.Title = model.Title;
            job.Description = model.Description;
            job.Requirements = model.Requirements;
            job.Responsibilities = model.Responsibilities;
            job.Benefits = model.Benefits;
            job.RequiredSkills = model.RequiredSkills;
            job.Education = model.Education;

            job.ExperienceLevel = model.ExperienceLevel;
            job.ExperienceYears = model.ExperienceYears;

            job.Category = model.Category;
            job.Location = model.Location;

            job.JobType = (JobType)model.JobType;
            job.WorkArrangement = (WorkArrangement)model.WorkArrangement;

            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;

            job.Status = model.Status;

            job.ClosingDate = model.ClosingDate;

            // Do not change:
            // job.JobId
            // job.EmployerId
            // job.PostedDate

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"\"{job.Title}\" was updated successfully.";

            return RedirectToAction(nameof(Jobs));
        }


        // ============================================================
        // GET CURRENT EMPLOYER
        // ============================================================

        private async Task<Employer?> GetCurrentEmployerAsync()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return null;

            return await _context.Employers
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }


        // ============================================================
        // POPULATE DROPDOWN LISTS
        // ============================================================

        private static void PopulateSelectLists(PostJobViewModel vm)
        {
            vm.JobTypeOptions =
                EnumSelectListHelper.GetSelectList<JobType>();

            vm.WorkArrangementOptions =
                EnumSelectListHelper.GetSelectList<WorkArrangement>();

            vm.ExperienceLevelOptions =
                EnumSelectListHelper.GetSelectList<ExperienceLevel>(
                    includeEmpty: true,
                    emptyText: "Not specified");

            // Only Draft and Active are available
            // when creating/editing through this form.
            vm.StatusOptions =
                EnumSelectListHelper.GetSelectList<JobStatus>()
                    .Where(s =>
                        s.Value == nameof(JobStatus.Draft) ||
                        s.Value == nameof(JobStatus.Active))
                    .ToList();
        }
    }
}
