using CRMS.Data;
using CRMS.Models;
using CRMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Security.Claims;
using CRMS.Services;

namespace CRMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IActivityLogService _activityLogService;

        public AccountController(UserManager<Users> userManager, SignInManager<Users> signInManager, AppDbContext context, IActivityLogService activityLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _activityLogService = activityLogService;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            // Log the activity
            _activityLogService.LogActivityAsync(
                "View",
                "LoginForm",
                "Anonymous",
                "Viewed login form"
            ).Wait();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Email or Password is invalid");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Email or Password is invalid");
                return View(model);
            }

            // Check if it's the user's first login
            if (!user.FirstLogin)
            {
                return RedirectToAction("ResetPassword", "Account", new { email = user.Email });
            }

            TempData["LoginSuccess"] = true;
            TempData.Keep("LoginSuccess");

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Login",
                "User",
                user.Id,
                "User logged in successfully"
            );

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                Users user = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    CreatedAt = DateTime.UtcNow,
                    FirstLogin = false // Set first login to false by default
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Log the activity
                    await _activityLogService.LogActivityAsync(
                        "Register",
                        "User",
                        user.Id,
                        "New user registered"
                    );

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                Console.WriteLine($"Exception in Signup: {ex}"); // Log the exception
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Update FirstLogin to true
            user.FirstLogin = true;
            await _userManager.UpdateAsync(user);

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Logout",
                "User",
                user.Id,
                "User logged out"
            );

            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "Profile",
                user.Id,
                "Viewed user profile"
            );

            // Get all teams where user is a member or leader
            var teams = await _context.Teams
                .Include(t => t.TeamMembers)
                .Include(t => t.CaseTeams)
                    .ThenInclude(ct => ct.Case)
                .Where(t => t.TeamLeaderId == user.Id || t.TeamMembers.Any(tm => tm.UserId == user.Id))
                .ToListAsync();

            // Get all cases assigned to these teams
            var assignedCases = teams
                .SelectMany(t => t.CaseTeams)
                .Select(ct => new
                {
                    ct.Case.Id,
                    ct.Case.Title,
                    ct.Case.Description,
                    ct.Case.Status,
                    ct.Case.Priority,
                    ct.Case.CreatedDate,
                    ct.AssignedDate,
                    TeamName = ct.Team.Name,
                    IsTeamLeader = ct.Team.TeamLeaderId == user.Id
                })
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            ViewBag.AssignedCases = assignedCases;
            return View(user);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Users model)
        {
            if (!ModelState.IsValid)
            {
                return View("Profile", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            // Update user properties
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View("Profile", model);
        }
    }
}
