using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using static JobFinder.Models.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace JobFinder.Controllers
{
    [Authorize(Roles = "Employer")]
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
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var employerData = await _context.Employers
                .Include(e => e.Jobs)
                    .ThenInclude(j => j.Applications)
                .FirstOrDefaultAsync(
                    e => e.EmployerId == employer.EmployerId);

            if (employerData is null)
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var applications = employerData.Jobs
                .SelectMany(j => j.Applications)
                .ToList();

            var vm = new EmployerDashboardViewModel
            {
                CompanyName = employerData.CompanyName,
                IsVerified = employerData.IsVerified,

                // ====================================================
                // JOB STATISTICS
                // ====================================================

                ActiveJobsCount = employerData.Jobs
                    .Count(j => j.Status == JobStatus.Active),

                DraftJobsCount = employerData.Jobs
                    .Count(j => j.Status == JobStatus.Draft),

                // ====================================================
                // APPLICATION STATISTICS
                // ====================================================

                TotalApplicantsCount = applications.Count,

                ShortlistedCount = applications
                    .Count(a =>
                        a.Status == ApplicationStatus.Shortlisted),

                InterviewCount = applications
                    .Count(a =>
                        a.Status == ApplicationStatus.Interview),

                SubmittedCount = applications
                    .Count(a =>
                        a.Status == ApplicationStatus.Submitted),

                WithdrawnCount = applications
                    .Count(a =>
                        a.Status == ApplicationStatus.Withdrawn),

                PendingReviewCount = applications
                    .Count(a =>
                        a.Status == ApplicationStatus.Pending),

                // ====================================================
                // RECENT JOBS
                // ====================================================

                RecentJobs = employerData.Jobs
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
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var model = new PostJobViewModel();

            PopulateSelectLists(model);

            return View(model);
        }


        // ============================================================
        // POST JOB - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(
            PostJobViewModel model)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            // ========================================================
            // SALARY VALIDATION
            // ========================================================

            if (model.SalaryMin.HasValue &&
                model.SalaryMax.HasValue &&
                model.SalaryMax.Value < model.SalaryMin.Value)
            {
                ModelState.AddModelError(
                    nameof(model.SalaryMax),
                    "Maximum salary must be at least the minimum salary.");
            }

            // ========================================================
            // CLOSING DATE VALIDATION
            // ========================================================

            if (model.ClosingDate.HasValue &&
                model.ClosingDate.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(
                    nameof(model.ClosingDate),
                    "Closing date can't be in the past.");
            }

            // ========================================================
            // JOB STATUS VALIDATION
            // ========================================================

            if (model.Status != JobStatus.Draft &&
                model.Status != JobStatus.Active)
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "New postings must be Draft or Active.");
            }

            // ========================================================
            // RETURN FORM IF INVALID
            // ========================================================

            if (!ModelState.IsValid)
            {
                PopulateSelectLists(model);
                return View(model);
            }

            // ========================================================
            // CREATE JOB
            // ========================================================

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

                WorkArrangement =
                    (WorkArrangement)model.WorkArrangement,

                SalaryMin = model.SalaryMin,
                SalaryMax = model.SalaryMax,

                Status = model.Status,

                ClosingDate = model.ClosingDate,

                PostedDate = DateTime.UtcNow
            };

            _context.Jobs.Add(job);

            await _context.SaveChangesAsync();

            if (model.Status == JobStatus.Active)
            {
                TempData["Success"] =
                    $"\"{job.Title}\" was posted and is now live.";
            }
            else
            {
                TempData["Success"] =
                    $"\"{job.Title}\" was saved as a draft.";
            }

            return RedirectToAction(nameof(Dashboard));
        }


        // ============================================================
        // JOBS
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
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var query = _context.Jobs
                .Where(j =>
                    j.EmployerId == employer.EmployerId)
                .Include(j => j.Applications)
                .AsQueryable();

            // ========================================================
            // SEARCH
            // ========================================================

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(j =>
                    j.Title.Contains(search) ||
                    j.Description.Contains(search) ||
                    (j.Category != null &&
                     j.Category.Contains(search)));
            }

            // ========================================================
            // STATUS FILTER
            // ========================================================

            if (!string.IsNullOrWhiteSpace(status))
            {
                status = status.Trim();

                query = query.Where(j =>
                    j.Status.ToString() == status);
            }

            // ========================================================
            // CATEGORY FILTER
            // ========================================================

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
        // JOB DETAILS
        // Displays full details of an employer's own job
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> JobDetails(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var job = await _context.Jobs
                .Include(j => j.Employer)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employer.EmployerId);

            if (job is null)
            {
                return NotFound(
                    "Job posting not found or you do not have permission to view it.");
            }

            return View(job);
        }


        // ============================================================
        // EDIT JOB - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> EditJob(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employer.EmployerId);

            if (job is null)
            {
                return NotFound(
                    "Job posting not found or you do not have permission to edit it.");
            }

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
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

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
            // SALARY VALIDATION
            // ========================================================

            if (model.SalaryMin.HasValue &&
                model.SalaryMax.HasValue &&
                model.SalaryMax.Value < model.SalaryMin.Value)
            {
                ModelState.AddModelError(
                    nameof(model.SalaryMax),
                    "Maximum salary must be at least the minimum salary.");
            }

            // ========================================================
            // CLOSING DATE VALIDATION
            // ========================================================

            if (model.ClosingDate.HasValue &&
                model.ClosingDate.Value.Date < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(
                    nameof(model.ClosingDate),
                    "Closing date can't be in the past.");
            }

            // ========================================================
            // JOB STATUS VALIDATION
            // ========================================================

            if (!Enum.IsDefined(
                    typeof(JobStatus),
                    model.Status))
            {
                ModelState.AddModelError(
                    nameof(model.Status),
                    "Invalid job status.");
            }

            // ========================================================
            // RETURN FORM IF INVALID
            // ========================================================

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

            job.WorkArrangement =
                (WorkArrangement)model.WorkArrangement;

            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;

            job.Status = model.Status;

            job.ClosingDate = model.ClosingDate;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"\"{job.Title}\" was updated successfully.";

            return RedirectToAction(nameof(Jobs));
        }


        // ============================================================
        // DELETE JOB - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var job = await _context.Jobs
                .Include(j => j.Employer)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employer.EmployerId);

            if (job is null)
            {
                return NotFound(
                    "Job posting not found or you do not have permission to delete it.");
            }

            return View(job);
        }


        // ============================================================
        // DELETE JOB - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteJobConfirmed(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var job = await _context.Jobs
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employer.EmployerId);

            if (job is null)
            {
                return NotFound(
                    "Job posting not found or you do not have permission to delete it.");
            }

            // ========================================================
            // PROTECT APPLICATION HISTORY
            // ========================================================

            if (job.Applications.Any())
            {
                TempData["Error"] =
                    "This job cannot be deleted because it has received " +
                    "applications. You can close the job instead.";

                return RedirectToAction(
                    nameof(JobDetails),
                    new { id = job.JobId });
            }

            var jobTitle = job.Title;

            _context.Jobs.Remove(job);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"\"{jobTitle}\" was deleted successfully.";

            return RedirectToAction(nameof(Jobs));
        }


        // ============================================================
        // JOB APPLICANTS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> JobApplicants(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found.");
            }

            // ========================================================
            // VERIFY JOB OWNERSHIP
            // ========================================================

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.JobId == id &&
                    j.EmployerId == employer.EmployerId);

            if (job is null)
            {
                return NotFound(
                    "Job not found.");
            }

            // ========================================================
            // GET APPLICATIONS
            // ========================================================

            var applications = await _context.Applications
                .Where(a => a.JobId == id)
                .Include(a => a.Applicant)
                    .ThenInclude(a => a.User)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            // ========================================================
            // BUILD VIEW MODEL
            // ========================================================

            var viewModel = new JobApplicantsViewModel
            {
                JobId = job.JobId,

                JobTitle = job.Title,

                TotalApplicants = applications.Count,

                Applicants = applications
                    .Select(a => new JobApplicantRowViewModel
                    {
                        ApplicationId = a.ApplicationId,

                        ApplicantId = a.ApplicantId,

                        FullName =
                            a.Applicant.User.FullName,

                        Email =
                            a.Applicant.User.Email
                            ?? string.Empty,

                        PhoneNumber =
                            a.Applicant.PhoneNumber,

                        Location =
                            a.Applicant.Location,

                        Skills =
                            a.Applicant.Skills,

                        Education =
                            a.Applicant.Education,

                        ExperienceLevel =
                            a.Applicant.ExperienceLevel?.ToString(),

                        ExperienceYears =
                            a.Applicant.ExperienceYears,

                        AIMatchScore =
                            a.AIMatchScore,

                        Status =
                            a.Status,

                        ApplicationDate =
                            a.ApplicationDate,

                        ResumeUrl =
                            a.ResumeUrl ??
                            a.Applicant.ResumeUrl,

                        CoverLetter =
                            a.CoverLetter,

                        ReviewedDate =
                            a.ReviewedDate
                    })
                    .ToList()
            };

            return View(viewModel);
        }


        // ============================================================
        // ALL APPLICANTS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Applicants()
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found for this account.");
            }

            var applications = await _context.Applications
                .Where(a =>
                    a.Job.EmployerId == employer.EmployerId)
                .Include(a => a.Applicant)
                    .ThenInclude(a => a.User)
                .Include(a => a.Job)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            var applicants = applications
                .Select(a => new JobApplicantRowViewModel
                {
                    ApplicationId =
                        a.ApplicationId,

                    ApplicantId =
                        a.ApplicantId,

                    FullName =
                        a.Applicant.User.FullName,

                    Email =
                        a.Applicant.User.Email
                        ?? string.Empty,

                    PhoneNumber =
                        a.Applicant.PhoneNumber,

                    Location =
                        a.Applicant.Location,

                    Skills =
                        a.Applicant.Skills,

                    Education =
                        a.Applicant.Education,

                    ExperienceLevel =
                        a.Applicant.ExperienceLevel?.ToString(),

                    ExperienceYears =
                        a.Applicant.ExperienceYears,

                    AIMatchScore =
                        a.AIMatchScore,

                    Status =
                        a.Status,

                    ApplicationDate =
                        a.ApplicationDate,

                    ResumeUrl =
                        a.ResumeUrl ??
                        a.Applicant.ResumeUrl,

                    CoverLetter =
                        a.CoverLetter,

                    ReviewedDate =
                        a.ReviewedDate
                })
                .ToList();

            return View(applicants);
        }


        // ============================================================
        // APPLICANT DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ApplicantDetails(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found.");
            }

            // ========================================================
            // FIND APPLICATION
            // ========================================================

            var application = await _context.Applications
                .Include(a => a.Applicant)
                    .ThenInclude(a => a.User)
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.ApplicationId == id &&
                    a.Job.EmployerId == employer.EmployerId);

            if (application is null)
            {
                return NotFound(
                    "Application not found.");
            }

            var applicant = application.Applicant;

            // ========================================================
            // BUILD VIEW MODEL
            // ========================================================

            var viewModel = new ApplicantDetailsViewModel
            {
                // ====================================================
                // APPLICANT INFORMATION
                // ====================================================

                ApplicantId =
                    applicant.ApplicantId,

                FullName =
                    applicant.User.FullName,

                Email =
                    applicant.User.Email
                    ?? string.Empty,

                PhoneNumber =
                    applicant.PhoneNumber,

                Location =
                    applicant.Location,

                Bio =
                    applicant.Bio,

                Skills =
                    applicant.Skills,

                Education =
                    applicant.Education,

                Certifications =
                    applicant.Certifications,

                ExperienceLevel =
                    applicant.ExperienceLevel,

                ExperienceYears =
                    applicant.ExperienceYears,

                ResumeUrl =
                    application.ResumeUrl ??
                    applicant.ResumeUrl,

                PortfolioUrl =
                    applicant.PortfolioUrl,

                PhotoUrl =
                    applicant.PhotoUrl,

                // ====================================================
                // APPLICATION INFORMATION
                // ====================================================

                ApplicationId =
                    application.ApplicationId,

                JobId =
                    application.JobId,

                JobTitle =
                    application.Job.Title,

                ApplicationDate =
                    application.ApplicationDate,

                Status =
                    application.Status,

                AIMatchScore =
                    application.AIMatchScore,

                CoverLetter =
                    application.CoverLetter,

                ReviewedDate =
                    application.ReviewedDate,

                EmployerNotes =
                    application.EmployerNotes
            };

            return View(viewModel);
        }


        // ============================================================
        // SHORTLIST APPLICANT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ShortlistApplicant(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found.");
            }

            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.ApplicationId == id &&
                    a.Job.EmployerId == employer.EmployerId);

            if (application is null)
            {
                return NotFound(
                    "Application not found.");
            }

            // ========================================================
            // VALID STATUS TRANSITION
            // ========================================================

            if (application.Status != ApplicationStatus.Submitted &&
                application.Status != ApplicationStatus.UnderReview)
            {
                TempData["Error"] =
                    "This applicant cannot be shortlisted in their current status.";

                return RedirectToAction(
                    nameof(ApplicantDetails),
                    new
                    {
                        id = application.ApplicationId
                    });
            }

            application.Status =
                ApplicationStatus.Shortlisted;

            application.ReviewedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Applicant has been shortlisted successfully.";

            return RedirectToAction(
                nameof(ApplicantDetails),
                new
                {
                    id = application.ApplicationId
                });
        }


        // ============================================================
        // REMOVE FROM SHORTLIST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromShortlist(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found.");
            }

            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.ApplicationId == id &&
                    a.Job.EmployerId == employer.EmployerId);

            if (application is null)
            {
                return NotFound(
                    "Application not found.");
            }

            // ========================================================
            // ONLY SHORTLISTED APPLICANTS CAN BE REMOVED
            // ========================================================

            if (application.Status != ApplicationStatus.Shortlisted)
            {
                TempData["Error"] =
                    "This applicant is not currently shortlisted.";

                return RedirectToAction(
                    nameof(ApplicantDetails),
                    new
                    {
                        id = application.ApplicationId
                    });
            }

            // ========================================================
            // RETURN TO UNDER REVIEW
            // ========================================================

            application.Status =
                ApplicationStatus.UnderReview;

            application.ReviewedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Applicant has been removed from the shortlist.";

            return RedirectToAction(
                nameof(ApplicantDetails),
                new
                {
                    id = application.ApplicationId
                });
        }


        // ============================================================
        // SCHEDULE INTERVIEW
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleInterview(
            int id,
            DateTime interviewDate,
            InterviewType interviewType,
            string? meetingLink,
            string? notes)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found.");
            }

            // ========================================================
            // FIND APPLICATION AND VERIFY OWNERSHIP
            // ========================================================

            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.ApplicationId == id &&
                    a.Job.EmployerId == employer.EmployerId);

            if (application is null)
            {
                return NotFound(
                    "Application not found.");
            }

            // ========================================================
            // ONLY SHORTLISTED APPLICANTS CAN BE INTERVIEWED
            // ========================================================

            if (application.Status !=
                ApplicationStatus.Shortlisted)
            {
                TempData["Error"] =
                    "Only shortlisted applicants can be scheduled for an interview.";

                return RedirectToAction(
                    nameof(ApplicantDetails),
                    new
                    {
                        id = application.ApplicationId
                    });
            }

            // ========================================================
            // INTERVIEW DATE VALIDATION
            // ========================================================

            if (interviewDate <= DateTime.UtcNow)
            {
                TempData["Error"] =
                    "The interview date and time must be in the future.";

                return RedirectToAction(
                    nameof(ApplicantDetails),
                    new
                    {
                        id = application.ApplicationId
                    });
            }

            // ========================================================
            // PREVENT DUPLICATE SCHEDULED INTERVIEWS
            // ========================================================

            var existingInterview =
                await _context.Interviews
                    .AnyAsync(i =>
                        i.ApplicationId ==
                            application.ApplicationId &&
                        i.Status ==
                            InterviewStatus.Scheduled);

            if (existingInterview)
            {
                TempData["Error"] =
                    "This applicant already has a scheduled interview.";

                return RedirectToAction(
                    nameof(ApplicantDetails),
                    new
                    {
                        id = application.ApplicationId
                    });
            }

            // ========================================================
            // CREATE INTERVIEW
            // ========================================================

            var interview = new Interview
            {
                ApplicationId =
                    application.ApplicationId,

                InterviewDate =
                    interviewDate,

                InterviewType =
                    interviewType,

                MeetingLink =
                    meetingLink,

                Status =
                    InterviewStatus.Scheduled,

                Notes =
                    notes,

                CreatedDate =
                    DateTime.UtcNow
            };

            _context.Interviews.Add(interview);

            // ========================================================
            // UPDATE APPLICATION STATUS
            // ========================================================

            application.Status =
                ApplicationStatus.Interview;

            application.ReviewedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Interview has been scheduled successfully.";

            return RedirectToAction(
                nameof(ApplicantDetails),
                new
                {
                    id = application.ApplicationId
                });
        }


        // ============================================================
        // REJECT APPLICANT
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplicant(int id)
        {
            var employer = await GetCurrentEmployerAsync();

            if (employer is null)
            {
                return NotFound(
                    "Employer profile not found.");
            }

            var application = await _context.Applications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.ApplicationId == id &&
                    a.Job.EmployerId == employer.EmployerId);

            if (application is null)
            {
                return NotFound(
                    "Application not found.");
            }

            // ========================================================
            // PREVENT DUPLICATE REJECTION
            // ========================================================

            if (application.Status ==
                ApplicationStatus.Rejected)
            {
                TempData["Error"] =
                    "This applicant has already been rejected.";

                return RedirectToAction(
                    nameof(ApplicantDetails),
                    new
                    {
                        id = application.ApplicationId
                    });
            }

            // ========================================================
            // DO NOT REJECT ACTIVE INTERVIEW
            // ========================================================

            if (application.Status ==
                ApplicationStatus.Interview)
            {
                TempData["Error"] =
                    "This applicant already has an interview scheduled. " +
                    "Cancel the interview before rejecting the applicant.";

                return RedirectToAction(
                    nameof(ApplicantDetails),
                    new
                    {
                        id = application.ApplicationId
                    });
            }

            // ========================================================
            // REJECT APPLICATION
            // ========================================================

            application.Status =
                ApplicationStatus.Rejected;

            application.ReviewedDate =
                DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Applicant has been rejected.";

            return RedirectToAction(
                nameof(ApplicantDetails),
                new
                {
                    id = application.ApplicationId
                });
        }


        // ============================================================
        // GET CURRENT EMPLOYER
        // ============================================================

        private async Task<Employer?> GetCurrentEmployerAsync()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return await _context.Employers
                .FirstOrDefaultAsync(e =>
                    e.UserId == userId);
        }


        // ============================================================
        // POPULATE DROPDOWN LISTS
        // ============================================================

        private static void PopulateSelectLists(
            PostJobViewModel vm)
        {
            // ========================================================
            // JOB TYPE
            // ========================================================

            vm.JobTypeOptions =
                EnumSelectListHelper
                    .GetSelectList<JobType>();

            // ========================================================
            // WORK ARRANGEMENT
            // ========================================================

            vm.WorkArrangementOptions =
                EnumSelectListHelper
                    .GetSelectList<WorkArrangement>();

            // ========================================================
            // EXPERIENCE LEVEL
            // ========================================================

            vm.ExperienceLevelOptions =
                EnumSelectListHelper
                    .GetSelectList<ExperienceLevel>(
                        includeEmpty: true,
                        emptyText: "Not specified");

            // ========================================================
            // JOB STATUS
            // ========================================================

            vm.StatusOptions =
                EnumSelectListHelper
                    .GetSelectList<JobStatus>()
                    .Where(s =>
                        s.Value ==
                            nameof(JobStatus.Draft) ||
                        s.Value ==
                            nameof(JobStatus.Active))
                    .ToList();
        }
    }
}
