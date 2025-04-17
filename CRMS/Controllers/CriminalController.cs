using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CRMS.Services;
using System.Security.Claims;

namespace CRMS.Controllers
{
    [Route("Criminal")]
    public class CriminalController : Controller
    {
        private readonly AppDbContext context;
        private readonly IWebHostEnvironment environment;
        private readonly IActivityLogService _activityLogService;

        public CriminalController(
            AppDbContext context, 
            IWebHostEnvironment environment,
            IActivityLogService activityLogService)
        {
            this.context = context;
            this.environment = environment;
            _activityLogService = activityLogService;
        }

        // GET: Criminal/Viewlist
        [Route("Viewlist")]
        public IActionResult ViewList()
        {
            // Log the activity
            _activityLogService.LogActivityAsync(
                "View",
                "CriminalList",
                "All",
                "Viewed list of all criminals"
            ).Wait();

            var criminals = context.Criminal.OrderBy(p => p.Id).ToList();
            return View(criminals);
        }

        // GET: Criminal/Add
        [Route("Add")]
        public IActionResult Add()
        {
            // Log the activity
            _activityLogService.LogActivityAsync(
                "View",
                "CriminalAddForm",
                "New",
                "Viewed criminal creation form"
            ).Wait();

            return View();
        }

        // POST: Criminal/Add
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add(CriminalDTO criminalDTO)
        {
            // Validate the image file
            if (criminalDTO.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The Image is required");
            }

            // Validate the model state
            if (!ModelState.IsValid)
            {
                // Log the errors or inspect them
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return View(criminalDTO);
            }

            // Generate a unique filename
            string newfilename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(criminalDTO.ImageFile.FileName);

            // Save the image file
            string filepath = Path.Combine(environment.WebRootPath, "criminal_images", newfilename);
            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await criminalDTO.ImageFile.CopyToAsync(stream);
            }

            // Create new criminal
            var criminal = new Criminal
            {
                Id = Guid.NewGuid(),
                Name = criminalDTO.Name,
                Alias = criminalDTO.Alias,
                Age = criminalDTO.Age.GetValueOrDefault(),
                Gender = criminalDTO.Gender,
                Description = criminalDTO.Description,
                ImageFilename = newfilename,
                GangAffiliation = criminalDTO.GangAffiliation,
                Accomplices = criminalDTO.Accomplices,
                WeaponUsed = criminalDTO.WeaponUsed,
                KnownHabits = criminalDTO.KnownHabits,
                PsychologicalProfile = criminalDTO.PsychologicalProfile,
                Address = criminalDTO.Address,
                Caught = criminalDTO.Caught
            };

            context.Criminal.Add(criminal);
            await context.SaveChangesAsync();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Create",
                "Criminal",
                criminal.Id.ToString(),
                $"Created new criminal '{criminal.Name}'"
            );

            TempData["ToastMessage"] = $"Successfully added criminal: {criminal.Name}";
            TempData["ToastType"] = "success";
            return RedirectToAction("ViewList", "Criminal");
        }

        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var criminal = await context.Criminal
                .FirstOrDefaultAsync(c => c.Id == id);

            if (criminal == null)
            {
                return RedirectToAction("ViewList");
            }

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "Criminal",
                criminal.Id.ToString(),
                $"Viewed details of criminal '{criminal.Name}'"
            );

            return View(criminal);
        }

        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var criminal = await context.Criminal
                .FirstOrDefaultAsync(c => c.Id == id);

            if (criminal == null)
            {
                return RedirectToAction("ViewList");
            }

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "View",
                "CriminalEditForm",
                criminal.Id.ToString(),
                $"Viewed edit form for criminal '{criminal.Name}'"
            );

            var criminalDTO = new CriminalDTO
            {
                 //= criminal.Id,
                Name = criminal.Name,
                Alias = criminal.Alias,
                Age = criminal.Age,
                Gender = criminal.Gender,
                Description = criminal.Description,
                GangAffiliation = criminal.GangAffiliation,
                Accomplices = criminal.Accomplices,
                WeaponUsed = criminal.WeaponUsed,
                KnownHabits = criminal.KnownHabits,
                PsychologicalProfile = criminal.PsychologicalProfile,
                Address = criminal.Address,
                Caught = criminal.Caught
            };

            return View(criminalDTO);
        }

        [HttpPost]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id, CriminalDTO criminalDTO)
        {

            var criminal = await context.Criminal.FindAsync(id);
            if (criminal == null)
            {
                return RedirectToAction("ViewList");
            }

            string newfilename = criminal.ImageFilename;

            if (criminalDTO.ImageFile != null)
            {
                // Delete old image
                string oldfilepath = Path.Combine(environment.WebRootPath, "criminal_images", criminal.ImageFilename);
                if (System.IO.File.Exists(oldfilepath))
                {
                    System.IO.File.Delete(oldfilepath);
                }

                // Save new image
                newfilename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(criminalDTO.ImageFile.FileName);
                string newfilepath = Path.Combine(environment.WebRootPath, "criminal_images", newfilename);
                using (var stream = new FileStream(newfilepath, FileMode.Create))
                {
                    await criminalDTO.ImageFile.CopyToAsync(stream);
                }
            }

            // Update the criminal's properties
            criminal.Name = criminalDTO.Name;
            criminal.Alias = criminalDTO.Alias;
            criminal.Age = criminalDTO.Age.GetValueOrDefault();
            criminal.Gender = criminalDTO.Gender;
            criminal.Description = criminalDTO.Description;
            criminal.ImageFilename = newfilename;
            criminal.GangAffiliation = criminalDTO.GangAffiliation;
            criminal.Accomplices = criminalDTO.Accomplices;
            criminal.WeaponUsed = criminalDTO.WeaponUsed;
            criminal.KnownHabits = criminalDTO.KnownHabits;
            criminal.PsychologicalProfile = criminalDTO.PsychologicalProfile;
            criminal.Address = criminalDTO.Address;
            // Check if the criminal's caught status has changed
            if (criminal.Caught != criminalDTO.Caught)
            {
                criminal.Caught = criminalDTO.Caught;
                // Update bounty when criminal is caught
                await BountyService.UpdateCriminalBounty(context, criminal.Id);
            }
            else
            {
                criminal.Caught = criminalDTO.Caught;
            }

            // Save changes to the database
            await context.SaveChangesAsync();

            // Log the activity
            await _activityLogService.LogActivityAsync(
                "Update",
                "Criminal",
                criminal.Id.ToString(),
                $"Updated criminal '{criminal.Name}'"
            );

            TempData["ToastMessage"] = $"Successfully updated criminal: {criminal.Name}";
            TempData["ToastType"] = "success";
            return RedirectToAction("ViewList", "Criminal");
        }

        [HttpPost]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var criminal = await context.Criminal.FindAsync(id);
            if (criminal == null)
            {
                return RedirectToAction("ViewList");
            }

            // Delete the image file
            string filepath = Path.Combine(environment.WebRootPath, "criminal_images", criminal.ImageFilename);
            if (System.IO.File.Exists(filepath))
            {
                System.IO.File.Delete(filepath);
            }

            // Log the activity before deletion
            await _activityLogService.LogActivityAsync(
                "Delete",
                "Criminal",
                criminal.Id.ToString(),
                $"Deleted criminal '{criminal.Name}'"
            );

            context.Criminal.Remove(criminal);
            await context.SaveChangesAsync();

            TempData["ToastMessage"] = $"Successfully deleted criminal: {criminal.Name}";
            TempData["ToastType"] = "success";
            return RedirectToAction("ViewList", "Criminal");
        }
    }
}