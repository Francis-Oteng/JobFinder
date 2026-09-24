using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static JobFinder.Models.Enums;

namespace JobFinder.Controllers
{
    public class EmployerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public EmployerController(ApplicationDbContext context, UserManager<Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User)!;

            var employer = await _context.Employers
                .Include(e => e.Jobs).ThenInclude(j => j.Applications)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            var vm = new EmployerDashboardViewModel
            {
                CompanyName = employer.CompanyName,
                IsVerified = employer.IsVerified,
                ActiveJobsCount = employer.Jobs.Count(j => j.Status == JobStatus.Active),
                DraftJobsCount = employer.Jobs.Count(j => j.Status == JobStatus.Draft),
                TotalApplicantsCount = employer.Jobs.Sum(j => j.Applications.Count),
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
                RecentJobs = employer.Jobs
                    .OrderByDescending(j => j.PostedDate)
                    .Take(5)
                    .Select(j => new RecentJobRow
                    {
                        JobId = j.JobId,
                        Title = j.Title,
                        Status = j.Status.ToString(),
                        // Status = j.Status,
                        Category = j.Category,
                        Location = j.Location,
                        ApplicationsCount = j.Applications.Count,
                        PostedDate = j.PostedDate
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> PostJob()
        {
            var employer = await GetCurrentEmployerAsync();
            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            var vm = new PostJobViewModel();
            PopulateSelectLists(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(PostJobViewModel model)
        {
            var employer = await GetCurrentEmployerAsync();
            if (employer is null)
                return NotFound("Employer profile not found for this account.");

            if (model.SalaryMin.HasValue && model.SalaryMax.HasValue && model.SalaryMax < model.SalaryMin)
                ModelState.AddModelError(nameof(model.SalaryMax), "Maximum salary must be at least the minimum salary.");

            if (model.ClosingDate.HasValue && model.ClosingDate.Value.Date < DateTime.UtcNow.Date)
                ModelState.AddModelError(nameof(model.ClosingDate), "Closing date can't be in the past.");

            if (model.Status != JobStatus.Draft && model.Status != JobStatus.Active)
                ModelState.AddModelError(nameof(model.Status), "New postings must be Draft or Active.");

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
                JobType = model.JobType,
                WorkArrangement = model.WorkArrangement,
                SalaryMin = model.SalaryMin,
                SalaryMax = model.SalaryMax,
                Status = model.Status,
                ClosingDate = model.ClosingDate
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["Success"] = model.Status == JobStatus.Active
                ? $"\"{job.Title}\" was posted and is now live."
                : $"\"{job.Title}\" was saved as a draft.";

            return RedirectToAction(nameof(Dashboard));
        }

        private async Task<Employer?> GetCurrentEmployerAsync()
        {
            var userId = _userManager.GetUserId(User)!;
            return await _context.Employers.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        private static void PopulateSelectLists(PostJobViewModel vm)
        {
            vm.JobTypeOptions = EnumSelectListHelper.GetSelectList<JobType>();
            vm.WorkArrangementOptions = EnumSelectListHelper.GetSelectList<WorkArrangement>();
            vm.ExperienceLevelOptions = EnumSelectListHelper.GetSelectList<ExperienceLevel>(
                includeEmpty: true, emptyText: "Not specified");
            vm.StatusOptions = EnumSelectListHelper.GetSelectList<JobStatus>()
                .Where(s => s.Value == nameof(JobStatus.Draft) || s.Value == nameof(JobStatus.Active))
                .ToList();
        }
    }
}
