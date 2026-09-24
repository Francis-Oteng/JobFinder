
using JobFinder.Data;
using JobFinder.Models;
using JobFinder.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static JobFinder.Models.Enums;

namespace JobFinder.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Conditional requirements data annotations can't express cleanly.
            if (model.Role == UserRole.Employer)
            {
                if (string.IsNullOrWhiteSpace(model.CompanyName))
                    ModelState.AddModelError(nameof(model.CompanyName), "Company name is required.");
                if (string.IsNullOrWhiteSpace(model.Industry))
                    ModelState.AddModelError(nameof(model.Industry), "Industry is required.");
                if (string.IsNullOrWhiteSpace(model.EmployerLocation))
                    ModelState.AddModelError(nameof(model.EmployerLocation), "Location is required.");
            }
            if (model.Role == UserRole.Admin)
            {
                ModelState.AddModelError(nameof(model.Role), "Invalid role.");
            }

            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return View(model);
            }

            // Role claim drives [Authorize(Roles = "...")] without needing an
            // AspNetRoles table — read back automatically on every sign-in.
            await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, model.Role.ToString()));

            if (model.Role == UserRole.Applicant)
            {
                _context.Applicants.Add(new Applicant
                {
                    UserId = user.Id,
                    Location = model.ApplicantLocation,
                    PhoneNumber = model.ApplicantPhoneNumber
                });
            }
            else if (model.Role == UserRole.Employer)
            {
                _context.Employers.Add(new Employer
                {
                    UserId = user.Id,
                    CompanyName = model.CompanyName!,
                    Industry = model.Industry!,
                    Location = model.EmployerLocation!,
                    PhoneNumber = model.EmployerPhoneNumber,
                    IsVerified = false
                });
            }

            await _context.SaveChangesAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToRoleHome(model.Role);
        }


        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            user.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToRoleHome(user.Role);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToRoleHome(UserRole role) => role switch
        {
            UserRole.Applicant => RedirectToAction("Dashboard", "Applicant"),
            UserRole.Employer => RedirectToAction("Dashboard", "Employer"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}
    









