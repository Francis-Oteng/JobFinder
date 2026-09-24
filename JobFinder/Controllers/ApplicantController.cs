using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static JobFinder.Models.Enums;
using static System.Net.Mime.MediaTypeNames;
using Application = JobFinder.Models.Application;

namespace JobFinder.Controllers
{
    [Authorize(Roles = "Applicant")]
    public class ApplicantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicantController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // =========================================================
        // 1. APPLICANT DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var applicant = await _context.Applicants
                .Include(a => a.User)
                .Include(a => a.Applications)
                    .ThenInclude(app => app.Job)
                        .ThenInclude(j => j.Employer)
                .Include(a => a.SavedJobs)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found for this account.");

            var vm = new ApplicantDashboardViewModel
            {
                FullName = applicant.User.FullName,

                ProfileComplete =
                    !string.IsNullOrWhiteSpace(applicant.Skills)
                    && !string.IsNullOrWhiteSpace(applicant.Location)
                    && !string.IsNullOrWhiteSpace(applicant.ResumeUrl),

                ApplicationsCount =
                    applicant.Applications.Count,

                ShortlistedCount =
                    applicant.Applications.Count(
                        a => a.Status == ApplicationStatus.Shortlisted),

                InterviewCount =
                    applicant.Applications.Count(
                        a => a.Status == ApplicationStatus.Interview),

                SavedJobsCount =
                    applicant.SavedJobs.Count,

                RecentApplications =
                    applicant.Applications
                        .OrderByDescending(
                            a => a.ApplicationDate)
                        .Take(5)
                       .Select(a => new RecentApplicationRow 
                       { ApplicationId = a.ApplicationId,
                           JobId = a.JobId, 
                           JobTitle = a.Job.Title,
                           CompanyName = a.Job.Employer.CompanyName, 
                           Status = a.Status, 
                           ApplicationDate = a.ApplicationDate })
                        .ToList()
            };

            return View(vm);
        }


        // =========================================================
        // 2. EDIT PROFILE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(
                    a => a.UserId == user.Id);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found.");

            var model = new ApplicantProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,

                PhoneNumber = applicant.PhoneNumber,
                Location = applicant.Location,
                Bio = applicant.Bio,
                Skills = applicant.Skills,
                Education = applicant.Education,
                Certifications = applicant.Certifications,

                ExperienceLevel =
                    applicant.ExperienceLevel,

                ExperienceYears =
                    applicant.ExperienceYears,

                PreferredJobType =
                    applicant.PreferredJobType,

                ResumeUrl =
                    applicant.ResumeUrl,

                PortfolioUrl =
                    applicant.PortfolioUrl,

                PhotoUrl =
                    applicant.PhotoUrl
            };

            return View(model);
        }


        // =========================================================
        // 3. EDIT PROFILE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(
            ApplicantProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(
                    a => a.UserId == user.Id);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found.");

            // -----------------------------------------
            // Update Identity user information
            // -----------------------------------------

            user.FullName = model.FullName;

            // Update email and username if email changed
            if (!string.Equals(
                user.Email,
                model.Email,
                StringComparison.OrdinalIgnoreCase))
            {
                var emailResult =
                    await _userManager.SetEmailAsync(
                        user,
                        model.Email);

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }

                var usernameResult =
                    await _userManager.SetUserNameAsync(
                        user,
                        model.Email);

                if (!usernameResult.Succeeded)
                {
                    foreach (var error in usernameResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }
            }

            // -----------------------------------------
            // Update applicant information
            // -----------------------------------------

            applicant.PhoneNumber =
                model.PhoneNumber;

            applicant.Location =
                model.Location;

            applicant.Bio =
                model.Bio;

            applicant.Skills =
                model.Skills;

            applicant.Education =
                model.Education;

            applicant.Certifications =
                model.Certifications;

            applicant.ExperienceLevel =
                model.ExperienceLevel;

            applicant.ExperienceYears =
                model.ExperienceYears;

            applicant.PreferredJobType =
                model.PreferredJobType;

            applicant.ResumeUrl =
                model.ResumeUrl;

            applicant.PortfolioUrl =
                model.PortfolioUrl;

            applicant.PhotoUrl =
                model.PhotoUrl;

            applicant.UpdatedDate =
                DateTime.UtcNow;

            // -----------------------------------------
            // Save Identity user
            // -----------------------------------------

            var result =
                await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            // -----------------------------------------
            // Save Applicant
            // -----------------------------------------

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your profile has been updated successfully.";

            return RedirectToAction(
                nameof(Dashboard));
        }


        // =========================================================
        // 4. AVAILABLE JOBS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Jobs(
            string? search,
            JobType? jobType,
            string? location,
            string? category)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(
                    a => a.UserId == userId);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found.");

            // -----------------------------------------
            // Get available jobs
            // -----------------------------------------

            var query = _context.Jobs
                .Include(j => j.Employer)
                .AsQueryable();

            // Only show jobs that have not closed
            query = query.Where(
                j => j.ClosingDate >= DateTime.UtcNow);

            // -----------------------------------------
            // Search
            // -----------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(j =>
                    j.Title.Contains(search)
                    || j.Description.Contains(search)
                    || j.Category.Contains(search)
                    || j.Employer.CompanyName.Contains(search));
            }

            // -----------------------------------------
            // Job type filter
            // -----------------------------------------

            if (jobType.HasValue)
            {
                query = query.Where(
                    j => j.JobType == jobType.Value);
            }

            // -----------------------------------------
            // Location filter
            // -----------------------------------------

            if (!string.IsNullOrWhiteSpace(location))
            {
                location = location.Trim();

                query = query.Where(
                    j => j.Location.Contains(location));
            }

            // -----------------------------------------
            // Category filter
            // -----------------------------------------

            if (!string.IsNullOrWhiteSpace(category))
            {
                category = category.Trim();

                query = query.Where(
                    j => j.Category.Contains(category));
            }

            // -----------------------------------------
            // Build job list
            // -----------------------------------------

            var jobs = await query
                .OrderByDescending(
                    j => j.PostedDate)
                .Select(j => new JobOpportunityViewModel
                {
                    JobId = j.JobId,

                    Title = j.Title,

                    Description = j.Description,

                    Category = j.Category,

                    Location = j.Location,

                    JobType = j.JobType,

                    SalaryMin = j.SalaryMin,

                    SalaryMax = j.SalaryMax,

                    PostedDate = j.PostedDate,

                    ClosingDate = (DateTime)j.ClosingDate,

                    CompanyName =
                        j.Employer.CompanyName,

                    CompanyLogoUrl =
                        j.Employer.LogoUrl,

                    IsSaved =
                        _context.SavedJobs.Any(
                            s =>
                                s.ApplicantId ==
                                applicant.ApplicantId
                                && s.JobId == j.JobId),

                    HasApplied =
                        _context.Applications.Any(
                            a =>
                                a.ApplicantId ==
                                applicant.ApplicantId
                                && a.JobId == j.JobId)
                })
                .ToListAsync();

            var model = new ApplicantJobsViewModel
            {
                Search = search,

                JobType = jobType,

                Location = location,

                Category = category,

                Jobs = jobs
            };

            return View(model);
        }


        // =========================================================
        // 5. JOB DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> JobDetails(
            int id)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(
                    a => a.UserId == userId);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found.");

            var job = await _context.Jobs
                .Include(j => j.Employer)
                .FirstOrDefaultAsync(
                    j => j.JobId == id);

            if (job == null)
                return NotFound(
                    "Job not found.");

            var model = new JobDetailsViewModel
            {
                JobId = job.JobId,

                Title = job.Title,

                Description = job.Description,

                Category = job.Category,

                Location = job.Location,

                JobType = job.JobType,

                SalaryMin = job.SalaryMin,

                SalaryMax = job.SalaryMax,

                PostedDate = job.PostedDate,

                ClosingDate = (DateTime)job.ClosingDate,

                CompanyName =
                    job.Employer.CompanyName,

                CompanyDescription =
                    job.Employer.Description,

                CompanyLogoUrl =
                    job.Employer.LogoUrl,

                CompanyWebsite =
                    job.Employer.Website,

                IsSaved =
                    await _context.SavedJobs.AnyAsync(
                        s =>
                            s.ApplicantId ==
                            applicant.ApplicantId
                            && s.JobId == job.JobId),

                HasApplied =
                    await _context.Applications.AnyAsync(
                        a =>
                            a.ApplicantId ==
                            applicant.ApplicantId
                            && a.JobId == job.JobId)
            };

            return View(model);
        }


        // =========================================================
        // 6. APPLY FOR JOB
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(
            int jobId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(
                    a => a.UserId == userId);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found.");

            var job = await _context.Jobs
                .FirstOrDefaultAsync(
                    j => j.JobId == jobId);

            if (job == null)
                return NotFound(
                    "Job not found.");

            // -----------------------------------------
            // Check application deadline
            // -----------------------------------------

            if (job.ClosingDate < DateTime.UtcNow)
            {
                TempData["ErrorMessage"] =
                    "This job opportunity is no longer accepting applications.";

                return RedirectToAction(
                    nameof(JobDetails),
                    new { id = jobId });
            }

            // -----------------------------------------
            // Check duplicate application
            // -----------------------------------------

            var alreadyApplied =
                await _context.Applications.AnyAsync(
                    a =>
                        a.ApplicantId ==
                        applicant.ApplicantId
                        && a.JobId == jobId);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] =
                    "You have already applied for this job.";

                return RedirectToAction(
                    nameof(JobDetails),
                    new { id = jobId });
            }

            // -----------------------------------------
            // Create application
            // -----------------------------------------

            var application = new Application
            {
                ApplicantId =
                    applicant.ApplicantId,

                JobId =
                    jobId,

                ApplicationDate =
                    DateTime.UtcNow,

                Status =
                    ApplicationStatus.Submitted
            };

            _context.Applications.Add(
                application);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your application has been submitted successfully.";

            return RedirectToAction(
                nameof(JobDetails),
                new { id = jobId });
        }


        // =========================================================
        // 7. SAVE / UNSAVE JOB
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSaveJob(
            int jobId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(
                    a => a.UserId == userId);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found.");

            var job = await _context.Jobs
                .FirstOrDefaultAsync(
                    j => j.JobId == jobId);

            if (job == null)
                return NotFound(
                    "Job not found.");

            // -----------------------------------------
            // Check if already saved
            // -----------------------------------------

            var savedJob =
                await _context.SavedJobs
                    .FirstOrDefaultAsync(
                        s =>
                            s.ApplicantId ==
                            applicant.ApplicantId
                            && s.JobId == jobId);

            // -----------------------------------------
            // Remove saved job
            // -----------------------------------------

            if (savedJob != null)
            {
                _context.SavedJobs.Remove(
                    savedJob);

                TempData["SuccessMessage"] =
                    "Job removed from your saved jobs.";
            }

            // -----------------------------------------
            // Save job
            // -----------------------------------------

            else
            {
                var newSavedJob = new SavedJob
                {
                    ApplicantId =
                        applicant.ApplicantId,

                    JobId =
                        jobId
                };

                _context.SavedJobs.Add(
                    newSavedJob);

                TempData["SuccessMessage"] =
                    "Job saved successfully.";
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(JobDetails),
                new { id = jobId });
        }


        // =========================================================
        // 8. MY APPLICATIONS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Applications()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(
                    a => a.UserId == userId);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found.");

            var applications =
                await _context.Applications
                    .Where(a =>
                        a.ApplicantId ==
                        applicant.ApplicantId)
                    .Include(a => a.Job)
                        .ThenInclude(j => j.Employer)
                    .OrderByDescending(
                        a => a.ApplicationDate)
                    .ToListAsync();

            return View(applications);
        }


        // =========================================================
        // 9. SAVED JOBS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> SavedJobs()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var applicant = await _context.Applicants
                .FirstOrDefaultAsync(
                    a => a.UserId == userId);

            if (applicant == null)
                return NotFound(
                    "Applicant profile not found.");

            var savedJobs =
                await _context.SavedJobs
                    .Where(s =>
                        s.ApplicantId ==
                        applicant.ApplicantId)
                    .Include(s => s.Job)
                        .ThenInclude(j => j.Employer)
                    .OrderByDescending(
                        s => s.SavedJobId)
                    .ToListAsync();

            return View(savedJobs);
        }
    }
}
