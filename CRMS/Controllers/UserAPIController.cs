using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Net;
using CRMS.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Threading.Tasks;


namespace CRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly EmailService _emailService;

        [ActivatorUtilitiesConstructor] // Fix for multiple constructor error
        public UserAPIController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, SignInManager<Users> signInManager, EmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    MemberSince = user.CreatedAt
                });
            }

            return Ok(userList);
        }

        // Create User with Role and Send 
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest(new { message = "User already exists" });

            var user = new Users
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true
            };

            string generatedPassword = GenerateRandomPassword();
            var result = await _userManager.CreateAsync(user, generatedPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            if (!await _roleManager.RoleExistsAsync(model.Role))
                await _roleManager.CreateAsync(new IdentityRole(model.Role));

            await _userManager.AddToRoleAsync(user, model.Role);
            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.SetLockoutEnabledAsync(user, false);
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, generatedPassword);
            await _userManager.UpdateAsync(user);

            await _emailService.SendEmailAsync(user.Email, generatedPassword);

            return Ok(new { message = "User created successfully" });
        }

        // Get all available roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return Ok(roles);
        }

        // Delete User
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User deleted successfully" });
        }

        // Generate Random Password
        private string GenerateRandomPassword()
        {
            return Guid.NewGuid().ToString().Substring(0, 8) + "@Abc1"; // Example: 8 char random password
        }

        // Send Email with Password
}

// DTO for User Creation
public class CreateUserDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}
